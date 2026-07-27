using Cosmere.System.Roshar.Surgebinding.Ability.Transformation;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.Soulcasting;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetDisabledWorkTypes))]
public static class SoulcastWorkTypeDisabledPatch {
    [HarmonyPostfix]
    public static void Postfix(Pawn __instance, ref List<WorkTypeDef> __result) {
        WorkTypeDef? soulcastWork = RosharWorkTypeDefOf.Cosmere_Roshar_WorkType_Soulcasting;
        if (soulcastWork == null) return;
        if (__result.Contains(soulcastWork)) return;

        if (__instance.abilities != null) {
            List<Ability> abilities = __instance.abilities.AllAbilitiesForReading;
            for (int i = 0; i < abilities.Count; i++) {
                if (abilities[i] is Soulcast) return;
            }
        }

        __result.Add(soulcastWork);
    }
}
