using System;
using System.Reflection;
using System.Reflection.Emit;
using Cosmere.Framework.Util;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Framework.Patch.InnerStorage;

[HarmonyPatch(typeof(HaulAIUtility), nameof(HaulAIUtility.HaulToStorageJob))]
public static class PatchHaulToContainerJob {
    private static bool patched;

    [HarmonyPrepare]
    public static bool Prepare() {
        LongEventHandler.ExecuteWhenFinished(() => {
                if (!patched) Log.Warning("HaulAIUtility.HaulToStorageJob transpiler could not be applied.");
            }
        );
        return true;
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> AddInnerStorageCheck(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    ) {
        patched = false;
        MethodInfo? logErrorMethod = AccessTools.Method(typeof(Log), nameof(Log.Error), [typeof(string)]);
        MethodInfo? haulMethod = AccessTools.Method(
            typeof(HaulUtility),
            nameof(HaulUtility.PawnHaulThingToInnerStorage),
            [typeof(Pawn), typeof(Verse.Thing), typeof(IHaulDestination)]
        );
        MethodInfo? tryFindStorage = AccessTools.Method(
            typeof(StoreUtility),
            nameof(StoreUtility.TryFindBestBetterStorageFor)
        );
        Type innerStorageType = typeof(Comp.Thing.InnerStorage);

        List<CodeInstruction> code = instructions.ToList();

        // Label to skip to if cast fails
        Label continueLabel = generator.DefineLabel();

        for (int i = 0; i < code.Count; i++) {
            if (i == 0) continue;

            if (!code[i].opcode.Equals(OpCodes.Isinst)) continue;
            if (!code[i].operand.Equals(typeof(ISlotGroupParent))) continue;
            if (patched) {
                Log.Warning(
                    "[Cosmere] Found more than one call to `isinst ISlotGroupParent` in HaulAIUtility.HaulToStorageJob. This is most likely due to another mod, and may result in unpredictable behavior."
                );
                continue;
            }

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
                    new CodeInstruction(OpCodes.Ldloc_2).WithLabels(continueLabel), // haulDestination
                ]
            );
            i += 8;
            patched = true;
        }

        return code;
    }
}