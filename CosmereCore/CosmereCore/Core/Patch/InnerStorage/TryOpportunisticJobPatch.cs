using System;
using System.Reflection;
using System.Reflection.Emit;
using Concord;
using Cosmere.Core.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Core.Patch.InnerStorage;

[Patch]
public abstract class TryOpportunisticJobPatch : Pawn_JobTracker {
    private static readonly MethodInfo? HaulMethod = typeof(HaulUtility).GetMethod(
        nameof(HaulUtility.PawnHaulThingToInnerStorage),
        [typeof(Pawn), typeof(Verse.Thing), typeof(IHaulDestination)]
    );

    private static readonly MethodInfo ParentThingGetter =
        typeof(Comp.Thing.InnerStorage).GetProperty("ParentThing")!.GetGetMethod();

    private static readonly MethodInfo PositionGetter =
        typeof(Verse.Thing).GetProperty("Position")!.GetGetMethod();

    private static readonly FieldInfo? JobTrackerPawn = typeof(Pawn_JobTracker).GetField(
        "pawn",
        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
    );

    protected TryOpportunisticJobPatch(Pawn newPawn) : base(newPawn) { }

    [Inject(At.Transpiler, nameof(TryOpportunisticJob))]
    private static IEnumerable<CodeInstruction> AddInnerStorageCheck(
        IEnumerable<CodeInstruction> instructions,
        ITranspilerContext context
    ) {
        Type innerStorageType = typeof(Comp.Thing.InnerStorage);

        List<CodeInstruction> code = instructions.ToList();

        // qualified: Concord.Label collides with System.Reflection.Emit.Label, needed here for OpCodes.
        Concord.Label continueLabelOne = context.DefineLabel();
        Concord.Label continueLabelTwo = context.DefineLabel();

        LocalRef innerStorage = context.DeclareLocal(innerStorageType);
        LocalRef haulDestination = default;

        // Local, not a field: Concord reruns transpilers on every recompose of the target.
        int patchedCount = 0;

        for (int i = 0; i < code.Count; i++) {
            if (i == 0) continue;

            if (!code[i - 1].opcode.Equals(OpCodes.Ldloc_S)) continue;
            if (!code[i].opcode.Equals(OpCodes.Isinst)) continue;
            if (code[i].operand is not Type operand || operand != typeof(ISlotGroupParent)) continue;
            if (patchedCount > 2) {
                Logger.Warning(
                    "Found more than one call to `isinst ISlotGroupParent` in HaulAIUtility.HaulToStorageJob. This is most likely due to another mod, and may result in unpredictable behavior."
                );
                continue;
            }

            List<CodeInstruction> newInstructions = [];
            if (patchedCount == 0) {
                haulDestination = (LocalRef)code[i - 1].operand!;
                LocalRef intVec3 = (LocalRef)code[i + 3].operand!;
                object? skipLabel = code[i + 4].operand;
                newInstructions.AddRange(
                    [
                        new CodeInstruction(OpCodes.Isinst, innerStorageType), // is InnerStorage?
                        new CodeInstruction(OpCodes.Stloc_S, innerStorage), // Store in `innerStorage`
                        new CodeInstruction(OpCodes.Ldloc_S, innerStorage), // Load from `innerStorage`
                        new CodeInstruction(OpCodes.Brfalse_S, continueLabelOne), // skip if not

                        new CodeInstruction(OpCodes.Ldloc_S, innerStorage), // Load from `innerStorage`
                        new CodeInstruction(OpCodes.Call, ParentThingGetter),
                        new CodeInstruction(OpCodes.Call, PositionGetter),
                        new CodeInstruction(OpCodes.Stloc_S, intVec3),
                        new CodeInstruction(OpCodes.Br_S, skipLabel),
                        new CodeInstruction(OpCodes.Ldloc_S, haulDestination)
                            .WithLabels(continueLabelOne), // haulDestination
                    ]
                );
            }

            if (patchedCount == 1) {
                newInstructions.AddRange(
                    [
                        new CodeInstruction(OpCodes.Isinst, innerStorageType), // is InnerStorage?
                        new CodeInstruction(OpCodes.Brfalse_S, continueLabelTwo), // skip if not

                        new CodeInstruction(OpCodes.Ldarg_0), // p
                        new CodeInstruction(OpCodes.Ldfld, JobTrackerPawn),
                        new CodeInstruction(OpCodes.Ldloc_S, context.GetLocal(4)), // t
                        new CodeInstruction(
                            OpCodes.Ldloc_S,
                            haulDestination
                        ), // haulDestination (already IS InnerStorage)
                        new CodeInstruction(OpCodes.Call, HaulMethod),
                        new CodeInstruction(OpCodes.Ret),
                        new CodeInstruction(OpCodes.Ldloc_S, haulDestination)
                            .WithLabels(continueLabelTwo), // haulDestination
                    ]
                );
            }

            code.InsertRange(i, newInstructions);
            i += newInstructions.Count;
            patchedCount++;
        }

        // reported here, not a startup callback: ExecuteWhenFinished would read the flag too early.
        if (patchedCount != 2) {
            Logger.Warning(
                $"Pawn_JobTracker.TryOpportunisticJob transpiler expected 2 `isinst ISlotGroupParent` sites, found {patchedCount}."
            );
            return instructions;
        }

        return code;
    }
}
