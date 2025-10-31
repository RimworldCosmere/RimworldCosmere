using Verse;

namespace Cosmere.Extension;

public static class HediffExtension {
    public static bool ParentIsMissing(this Hediff hediff) {
        foreach (Hediff h in hediff.pawn.health.hediffSet.hediffs) {
            if (h is not Hediff_MissingPart) continue;
            if (h.Part == hediff.Part.parent) return true;
        }

        return false;
    }

    public static bool CanBeHealedByInvestiture(this Hediff hediff) {
        if (hediff is Hediff_Injury or Hediff_MissingPart) return true;
        if (hediff.def.chronic) return false;
        if (hediff.def.makesSickThought) return true;
        if (hediff.CurStage?.capMods?.Count > 0) return true;

        return false;
    }

    public static bool TryHealWithInvestiture(this Hediff hediff, float amount) {
        if (!hediff.CanBeHealedByInvestiture()) return false;

        if (hediff is Hediff_Injury injury) {
            injury.Heal(amount);
            return true;
        }

        if (hediff is Hediff_MissingPart) {
            return false;
        }

        hediff.Severity -= amount;
        if (hediff.Severity <= 0) {
            hediff.pawn.health.RemoveHediff(hediff);
        }

        return true;
    }
}
