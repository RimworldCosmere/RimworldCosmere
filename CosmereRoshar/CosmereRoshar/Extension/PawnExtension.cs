using Cosmere.Roshar.Def;
using Cosmere.Roshar.Gene;
using Verse;

namespace Cosmere.Roshar.Extension;

public static class PawnExtension {
    public static RadiantOrderDef? GetRadiantOrder(this Pawn pawn) {
        return pawn.genes?.GetFirstGeneOfType<Surgebinder>()?.radiantOrderDef;
    }

    public static bool IsPawnEligibleForDoctoring(this Pawn pawn) {
        if (pawn.Dead || pawn.AnimalOrWildMan() || pawn.NonHumanlikeOrWildMan()) {
            return false;
        }

        return !pawn.skills.GetSkill(RimWorld.SkillDefOf.Medicine).TotallyDisabled;
    }

    public static bool IsSurgebinder(this Pawn pawn) {
        return pawn.genes?.GetFirstGeneOfType<Surgebinder>() != null;
    }
}