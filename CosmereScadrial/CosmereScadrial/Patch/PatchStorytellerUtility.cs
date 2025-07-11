using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereScadrial.Patch;

[HarmonyPatch(typeof(StorytellerUtility), nameof(StorytellerUtility.DefaultThreatPointsNow))]
[HarmonyPatch([typeof(IIncidentTarget)])]
public static class PatchStorytellerUtility {
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codeList = instructions.ToList();
        MethodInfo getBonus = AccessTools.Method(typeof(PatchStorytellerUtility), nameof(GetThreatBonusForPawn));

        byte? pawnLocalIndex = null;
        bool injected = false;

        for (int i = 0; i < codeList.Count; i++) {
            CodeInstruction? current = codeList[i];
            CodeInstruction? previous = i > 0 ? codeList[i - 1] : null;

            // Try to detect and remember the pawn local variable (from get_Current())
            if (pawnLocalIndex == null && IsGettingCurrentPawn(previous)) {
                pawnLocalIndex = current.operand switch {
                    LocalBuilder lb => (byte)lb.LocalIndex,
                    int intIdx => (byte)intIdx,
                    byte b => b,
                    _ => throw new Exception("[Cosmere] Unknown operand type for pawn"),
                };
                // Emit original instruction
                yield return current;
                continue;
            }

            // Detect when num3 is being set after the if (item.IsFreeColonist)
            if (!injected &&
                current.opcode == OpCodes.Stloc_S &&
                previous?.opcode == OpCodes.Callvirt &&
                previous.operand is MethodInfo prevMethod &&
                prevMethod.Name == nameof(SimpleCurve.Evaluate)) {
                byte? num3LocalIndex = current.operand switch {
                    LocalBuilder lb => (byte)lb.LocalIndex,
                    int intIdx => (byte)intIdx,
                    byte b => b,
                    _ => throw new Exception("[Cosmere] Unknown operand type for num3"),
                };

                // Emit original instruction
                yield return current;

                // Inject: num3 = num3 + GetThreatBonusForPawn(item)
                yield return new CodeInstruction(OpCodes.Ldloc_S, num3LocalIndex);
                yield return new CodeInstruction(OpCodes.Ldloc_S, pawnLocalIndex);
                yield return new CodeInstruction(OpCodes.Call, getBonus);
                yield return new CodeInstruction(OpCodes.Add);
                yield return new CodeInstruction(OpCodes.Stloc_S, num3LocalIndex);

                injected = true;
                continue;
            }

            // Emit original instruction
            yield return current;
        }

        if (!injected) {
            throw new Exception("[Cosmere] Failed to inject GetThreatBonusForPawn logic");
        }

        yield break;

        static bool IsGettingCurrentPawn(CodeInstruction? code) {
            return code?.opcode == OpCodes.Callvirt &&
                   code.operand is MethodInfo method &&
                   method.Name == "get_Current" &&
                   method.DeclaringType?.IsGenericType == true &&
                   method.DeclaringType.GetGenericTypeDefinition() == typeof(IEnumerator<>) &&
                   method.DeclaringType.GetGenericArguments()[0] == typeof(Pawn);
        }
    }

    public static float GetThreatBonusForPawn(Pawn? pawn) {
        if (pawn?.genes == null || pawn.story?.traits == null || !pawn.IsColonist) return 0f;

        float bonus = 0f;
        if (pawn.IsMistborn() || pawn.IsFullFeruchemist()) {
            bonus += 150f;
        } else {
            List<Metalborn> metallicArtsGenes =
                pawn.genes.GetAllomanticGenes()
                    .Concat(pawn.genes.GetFeruchemicGenes().Cast<Metalborn>())
                    .Where(g => g != null)
                    .ToList();
            foreach (Metalborn gene in metallicArtsGenes) {
                switch (gene) {
                    // Keeping these separate so I can tweak them separately
                    case Allomancer:
                    case Feruchemist:
                        bonus += 20f;
                        break;
                }
            }
        }
        // @TODO add logic for Hemalurgic spikes

        return bonus;
    }
}