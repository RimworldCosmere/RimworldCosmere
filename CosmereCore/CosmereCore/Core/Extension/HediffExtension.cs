using Cosmere.Core.Framework;
using Verse;

namespace Cosmere.Core.Extension;

public static class HediffExtension {
    /// <summary>
    ///     Investiture can regrow a part lost recently, but an old amputation has settled into
    ///     who the pawn is and stays lost.
    /// </summary>
    private const int MissingPartHealWindowTicks = RimWorld.GenDate.TicksPerDay * 90;

    public static bool ParentIsMissing(this Verse.Hediff hediff) {
        return hediff.Part?.parent != null &&
               hediff.pawn.health.hediffSet.PartIsMissing(hediff.Part.parent);
    }

    public static bool CanBeHealedByInvestiture(this Verse.Hediff hediff) {
        if (hediff is Hediff_MissingPart missingPart) {
            return missingPart.ageTicks < MissingPartHealWindowTicks;
        }

        if (hediff is Hediff_Injury) return true;
        if (hediff.def.chronic) return false;
        if (InvestitureHealExclusionRegistry.IsExcluded(hediff)) return false;
        if (hediff.def.makesSickThought) return true;
        if (hediff.CurStage?.capMods?.Count > 0) return true;

        return false;
    }

    public static bool TryHealWithInvestiture(this Verse.Hediff hediff, float amount) {
        if (!hediff.CanBeHealedByInvestiture()) return false;

        if (hediff is Hediff_Injury injury) {
            injury.Heal(amount);
            return true;
        }

        hediff.Severity -= amount;
        if (hediff.Severity <= 0) {
            hediff.pawn.health.RemoveHediff(hediff);
        }

        return true;
    }
}
