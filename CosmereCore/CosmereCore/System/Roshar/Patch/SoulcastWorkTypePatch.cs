using Cosmere.System.Roshar.Surgebinding.Ability.Transformation;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.SetPriority))]
public static class SoulcastWorkTypePatch {
    private static WorkTypeDef? cachedWorkType;

    [HarmonyPrefix]
    public static bool Prefix(WorkTypeDef w, int priority, Pawn ___pawn) {
        if (priority != 0) return true;

        cachedWorkType ??= DefDatabase<WorkTypeDef>.GetNamedSilentFail("Cosmere_Roshar_WorkType_Soulcasting");
        if (cachedWorkType == null || w != cachedWorkType) return true;

        if (___pawn?.abilities == null) return true;

        List<Ability> abilities = ___pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i] is Soulcast) return false;
        }

        return true;
    }
}
