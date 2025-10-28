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
}