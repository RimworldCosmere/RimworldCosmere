using System;
using System.Reflection;
using System.Reflection.Emit;
using Cosmere.Core.Util;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Core.Patch.InnerStorage;

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.TryOpportunisticJob))]
public static class TryOpportunisticJobPatch {
    private static int patchedCount;

    [HarmonyPrepare]
    public static bool Prepare() {
        LongEventHandler.ExecuteWhenFinished(() => {
            if (patchedCount != 2) {
                Logger.Warning("Pawn_JobTracker.TryOpportunisticJob transpiler could not be applied.");
            }
        }
        );
        return true;
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> AddInnerStorageCheck(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    ) {
        patchedCount = 0;
        List<CodeInstruction> code = instructions.ToList();

        // Labels to skip to if cast fails
        Label continueLabelOne = generator.DefineLabel();
        Label continueLabelTwo = generator.DefineLabel();

        MethodInfo? haulMethod = AccessTools.Method(
            typeof(HaulUtility),
            nameof(HaulUtility.PawnHaulThingToInnerStorage),
            [typeof(Pawn), typeof(Verse.Thing), typeof(IHaulDestination)]
        );

        Type innerStorageType = typeof(Comp.Thing.InnerStorage);
        LocalBuilder innerStorage = generator.DeclareLocal(innerStorageType);

        LocalBuilder? haulDestination = null;
        LocalBuilder? intVec3 = null;

        for (int i = 0; i < code.Count; i++) {
            if (i == 0) continue;

            if (!code[i - 1].opcode.Equals(OpCodes.Ldloc_S)) continue;
            if (!code[i].opcode.Equals(OpCodes.Isinst)) continue;
            if (!code[i].operand.Equals(typeof(ISlotGroupParent))) continue;
            if (patchedCount > 2) {
                Logger.Warning(
                    "Found more than one call to `isinst ISlotGroupParent` in HaulAIUtility.HaulToStorageJob. This is most likely due to another mod, and may result in unpredictable behavior."
                );
                continue;
            }

            List<CodeInstruction> newInstructions = [];
            if (patchedCount == 0) {
                haulDestination = (LocalBuilder)code[i - 1].operand;
                intVec3 = (LocalBuilder)code[i + 3].operand;
                object? skipLabel = code[i + 4].operand;
                newInstructions.AddRange(
                    [
                        new CodeInstruction(OpCodes.Isinst, innerStorageType), // is InnerStorage?
                        new CodeInstruction(OpCodes.Stloc_S, innerStorage.LocalIndex), // Store in `innerStorage`
                        new CodeInstruction(OpCodes.Ldloc_S, innerStorage.LocalIndex), // Load from `innerStorage`
                        new CodeInstruction(OpCodes.Brfalse_S, continueLabelOne), // skip if not

                        new CodeInstruction(OpCodes.Ldloc_S, innerStorage.LocalIndex), // Load from `innerStorage`

                        // Call the getter for ParentThing
                        new CodeInstruction(
                            OpCodes.Call,
                            AccessTools.Property(typeof(Comp.Thing.InnerStorage), "ParentThing").GetGetMethod()
                        ),

                        // Call the getter for Position on the Thing
                        new CodeInstruction(
                            OpCodes.Call,
                            AccessTools.Property(typeof(Verse.Thing), "Position").GetGetMethod()
                        ),
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
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_JobTracker), "pawn")),
                        new CodeInstruction(OpCodes.Ldloc_S, 4), // t
                        new CodeInstruction(
                            OpCodes.Ldloc_S,
                            haulDestination
                        ), // haulDestination (already IS InnerStorage)

                        // new CodeInstruction(OpCodes.Castclass, typeof(IHaulDestination)),
                        new CodeInstruction(OpCodes.Call, haulMethod),
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

        return patchedCount == 2 ? code : instructions;
    }
}
