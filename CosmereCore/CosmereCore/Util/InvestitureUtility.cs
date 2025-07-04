using CosmereCore.Need;
using RimWorld;
using Verse;

namespace CosmereCore.Util;

public static class InvestitureUtility {
    // ReSharper disable once InconsistentNaming
    public static void AssignHeighteningFromBEUs(Pawn? pawn) {
        if (pawn?.story?.traits == null) {
            return;
        }

        RimWorld.Need? investNeed = pawn.needs?.TryGetNeed<Investiture>();
        if (investNeed == null) return;

        int degree = Investiture.GetDegreeFromBreathEquivalentUnits((int)investNeed.CurLevel);

        // Remove existing Invested trait (if present with different degree)
        Trait? existing = pawn.story.traits.GetTrait(TraitDefOf.Cosmere_Invested);
        if (existing != null && existing.Degree != degree) {
            pawn.story.traits.allTraits.Remove(existing);
        }

        // Add trait if not already correct
        if (!pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Invested)) {
            pawn.story.traits.GainTrait(new Trait(TraitDefOf.Cosmere_Invested, degree));
        }
    }
}