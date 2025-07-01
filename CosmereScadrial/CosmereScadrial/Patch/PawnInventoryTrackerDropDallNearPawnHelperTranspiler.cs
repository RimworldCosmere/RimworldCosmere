using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using ThingUtility = CosmereScadrial.Util.ThingUtility;

namespace CosmereScadrial.Patch;

[HarmonyPatch(typeof(Pawn_InventoryTracker), "DropAllNearPawnHelper")]
[HarmonyPatch(new[] { typeof(IntVec3), typeof(bool), typeof(bool), typeof(bool) })]
public static class PawnInventoryTrackerDropAllNearPawnHelperTranspiler {
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
        MethodInfo? addRange = AccessTools.Method(typeof(List<Verse.Thing>), nameof(List<Verse.Thing>.AddRange));
        MethodInfo? shouldDrop = AccessTools.Method(typeof(ThingUtility), nameof(ThingUtility.ShouldDrop));
        ConstructorInfo? funcCtor = typeof(Func<Verse.Thing, bool>).GetConstructor([typeof(object), typeof(IntPtr)]);

        MethodInfo where = typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m =>
                m.Name == "Where"
                && m.GetParameters().Length == 2
                && m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>)
            )
            .MakeGenericMethod(typeof(Verse.Thing));

        foreach (CodeInstruction? instruction in instructions) {
            // Find: list.AddRange(arg)
            if (instruction.Calls(addRange)) {
                // Instead of calling AddRange(arg)
                // transform arg => arg.Where(ShouldDrop)

                // Inject:
                // ldnull
                // ldftn ShouldDrop
                // newobj Func<Thing, bool>
                // call Enumerable.Where<Thing>

                yield return new CodeInstruction(OpCodes.Ldnull);
                yield return new CodeInstruction(OpCodes.Ldftn, shouldDrop);
                yield return new CodeInstruction(OpCodes.Newobj, funcCtor);
                yield return new CodeInstruction(OpCodes.Call, where);

                // Now call AddRange on the filtered list
                yield return instruction;
            } else {
                yield return instruction;
            }
        }
    }
}