using Cosmere.System.Roshar.Surgebinding.Ability.Transformation;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetDisabledWorkTypes))]
public static class SoulcastWorkTypeDisabledPatch {
    private static WorkTypeDef? cachedWorkType;

    [HarmonyPostfix]
    public static void Postfix(Pawn __instance, ref List<WorkTypeDef> __result) {
        cachedWorkType ??= DefDatabase<WorkTypeDef>.GetNamedSilentFail("Cosmere_Roshar_WorkType_Soulcasting");
        if (cachedWorkType == null) return;
        if (__result.Contains(cachedWorkType)) return;

        if (__instance.abilities != null) {
            List<Ability> abilities = __instance.abilities.AllAbilitiesForReading;
            for (int i = 0; i < abilities.Count; i++) {
                if (abilities[i] is Soulcast) return;
            }
        }

        __result.Add(cachedWorkType);
    }
}