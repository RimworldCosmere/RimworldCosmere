using System;
using System.Reflection;
using System.Reflection.Emit;
using Concord;
using Cosmere.Core.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Core.Patch.InnerStorage;

[Patch(typeof(HaulAIUtility))]
public static class HaulToContainerJobPatch {
    [Inject(At.Transpiler, nameof(HaulAIUtility.HaulToStorageJob))]
    private static IEnumerable<CodeInstruction> AddInnerStorageCheck(
        IEnumerable<CodeInstruction> instructions,
        ITranspilerContext context
    ) {
        MethodInfo? haulMethod = typeof(HaulUtility).GetMethod(
            nameof(HaulUtility.PawnHaulThingToInnerStorage),
            [typeof(Pawn), typeof(Verse.Thing), typeof(IHaulDestination)]
        );
        Type innerStorageType = typeof(Comp.Thing.InnerStorage);

        List<CodeInstruction> code = instructions.ToList();

        // qualified: Concord.Label collides with System.Reflection.Emit.Label, needed here for OpCodes.
        Concord.Label continueLabel = context.DefineLabel();

        // Local, not a field: Concord reruns transpilers on every recompose of the target.
        bool inserted = false;

        for (int i = 0; i < code.Count; i++) {
            if (i == 0) continue;

            if (!code[i].opcode.Equals(OpCodes.Isinst)) continue;
            if (code[i].operand is not Type operand || operand != typeof(ISlotGroupParent)) continue;
            if (inserted) {
                Logger.Warning(
                    "Found more than one call to `isinst ISlotGroupParent` in HaulAIUtility.HaulToStorageJob. This is most likely due to another mod, and may result in unpredictable behavior."
                );
                continue;
            }

            CodeInstruction resume = new CodeInstruction(OpCodes.Ldloc_2); // haulDestination
            resume.labels.Add(continueLabel);

            code.InsertRange(
                i,
                [
                    new CodeInstruction(OpCodes.Isinst, innerStorageType), // is InnerStorage?
                    new CodeInstruction(OpCodes.Brfalse_S, continueLabel), // skip if not

                    new CodeInstruction(OpCodes.Ldarg_0), // p
                    new CodeInstruction(OpCodes.Ldarg_1), // t
                    new CodeInstruction(OpCodes.Ldloc_2), // haulDestination (already IS InnerStorage)
                    new CodeInstruction(OpCodes.Call, haulMethod),
                    new CodeInstruction(OpCodes.Ret),
                    resume,
                ]
            );
            i += 8;
            inserted = true;
        }

        // reported here, not a startup callback: ExecuteWhenFinished would read the flag too early.
        if (!inserted) {
            Logger.Warning("HaulAIUtility.HaulToStorageJob transpiler found no `isinst ISlotGroupParent` to patch.");
            return instructions;
        }

        return code;
    }
}
