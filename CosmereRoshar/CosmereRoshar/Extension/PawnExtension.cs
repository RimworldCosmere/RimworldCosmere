using Cosmere.Roshar.Def;
using Cosmere.Roshar.Gene;
using Verse;

namespace Cosmere.Roshar.Extension;

public static class PawnExtension {
    public static RadiantOrderDef? GetRadiantOrder(this Pawn pawn) {
        foreach (Verse.Gene? gene in pawn.genes.GenesListForReading) {
            if (gene is not Surgebinder surgebinder) continue;

            return surgebinder.radiantOrderDef;
        }

        return null;
    }

    public static bool IsPawnEligibleForDoctoring(this Pawn pawn) {
        if (pawn == null || pawn.Dead || pawn.AnimalOrWildMan() || pawn.NonHumanlikeOrWildMan()) {
            return false;
        }

        return !pawn.skills.GetSkill(RimWorld.SkillDefOf.Medicine).TotallyDisabled;
    }
}