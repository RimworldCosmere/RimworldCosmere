using HarmonyLib;
using RimWorld;
using Verse;
using Cosmere.System.Roshar.Surgebinding.Ability.Transformation;

namespace Cosmere.System.Roshar.Patch.Soulcasting;

[HarmonyPatch(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.SetPriority))]
public static class SoulcastWorkTypePatch {
    [HarmonyPrefix]
    public static bool Prefix(WorkTypeDef w, int priority, Pawn ___pawn) {
        if (priority != 0) return true;

        WorkTypeDef? soulcastWork = RosharWorkTypeDefOf.Cosmere_Roshar_WorkType_Soulcasting;
        if (soulcastWork == null || w != soulcastWork) return true;

        if (___pawn?.abilities == null) return true;

        List<Ability> abilities = ___pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i] is Soulcast) return false;
        }

        return true;
    }
}