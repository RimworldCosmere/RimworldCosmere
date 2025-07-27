using System;
using System.Reflection;
using System.Reflection.Emit;
using Cosmere.Framework.Util;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Framework.Patch.InnerStorage;

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.TryOpportunisticJob))]
public static class PatchTryOpportunisticJob {
    private static int patchedCount;

    [HarmonyPrepare]
    public static bool Prepare() {
        LongEventHandler.ExecuteWhenFinished(() => {
                if (patchedCount != 2) Log.Warning("Pawn_JobTracker.TryOpportunisticJob transpiler could not be applied.");
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
        MethodInfo? haulMethod = AccessTools.Method(
            typeof(HaulUtility),
            nameof(HaulUtility.PawnHaulThingToInnerStorage),
            [typeof(Pawn), typeof(Verse.Thing), typeof(IHaulDestination)]
        );
        Type innerStorageType = typeof(Comp.Thing.InnerStorage);

        List<CodeInstruction> code = instructions.ToList();

        // Label to skip to if cast fails
        Label continueLabelOne = generator.DefineLabel();
        Label continueLabelTwo = generator.DefineLabel();
        LocalBuilder innerStorage = generator.DeclareLocal(typeof(Comp.Thing.InnerStorage));

        for (int i = 0; i < code.Count; i++) {
            if (i == 0) continue;

            if (!code[i].opcode.Equals(OpCodes.Isinst)) continue;
            if (!code[i].operand.Equals(typeof(ISlotGroupParent))) continue;
            if (patchedCount > 2) {
                Log.Warning(
                    "[Cosmere] Found more than one call to `isinst ISlotGroupParent` in HaulAIUtility.HaulToStorageJob. This is most likely due to another mod, and may result in unpredictable behavior."
                );
                continue;
            }

            List<CodeInstruction> newInstructions = new List<CodeInstruction>();
            if (patchedCount == 0) {
                object? intVec3 = code[i + 3].operand;
                newInstructions.AddRange(
                    [
                        new CodeInstruction(OpCodes.Isinst, innerStorageType), // is InnerStorage?
                        new CodeInstruction(OpCodes.Stloc_S, innerStorage.LocalIndex), // Store in `innerStorage`
                        new CodeInstruction(OpCodes.Ldloc_S, innerStorage.LocalIndex), // Load from `innerStorage`
                        new CodeInstruction(OpCodes.Brfalse_S, continueLabelOne), // skip if not

                        new CodeInstruction(OpCodes.Ldloc, innerStorage.LocalIndex), //Load from `innerStorage`
                        new CodeInstruction(OpCodes.Callvirt, AccessTools.Property(typeof(Comp.Thing.InnerStorage), "ParentThing").GetGetMethod()), //Call the getter for ParentThing
                        new CodeInstruction(OpCodes.Callvirt, AccessTools.Property(typeof(Verse.Thing), "Position").GetGetMethod()), // Call the getter for Position on the Thing
                        new CodeInstruction(OpCodes.Stloc_S, intVec3),
                        new CodeInstruction(OpCodes.Ldloc_2).WithLabels(continueLabelOne), // haulDestination
                    ]
                );
            }

            if (patchedCount == 1) {
                newInstructions.AddRange(
                    [
                        new CodeInstruction(OpCodes.Isinst, innerStorageType), // is InnerStorage?
                        new CodeInstruction(OpCodes.Brfalse_S, continueLabelTwo), // skip if not

                        new CodeInstruction(OpCodes.Ldarg_0), // p
                        new CodeInstruction(OpCodes.Ldarg_1), // t
                        new CodeInstruction(OpCodes.Ldloc_2), // haulDestination (already IS InnerStorage)
                        new CodeInstruction(OpCodes.Call, haulMethod),
                        new CodeInstruction(OpCodes.Ret),
                        new CodeInstruction(OpCodes.Ldloc_2).WithLabels(continueLabelTwo), // haulDestination
                    ]
                );
            }

            code.InsertRange(i, newInstructions);
            i += newInstructions.Count;
            patchedCount++;
        }

        return code;
    }
}