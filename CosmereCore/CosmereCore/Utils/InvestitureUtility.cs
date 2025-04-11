using RimWorld;
using Verse;

namespace CosmereCore.Utils
{
    public static class InvestitureUtility
    {
        // These thresholds match the canon Heightenings from Warbreaker
        private static readonly int[] BEU_THRESHOLDS =
        {
            1, // Degree 0: Invested
            100, // 1st Heightening
            200, // 2nd
            600, // 3rd
            1000, // 4th
            2000, // 5th
            3500, // 6th
            5000, // 7th
            6000, // 8th
            10000, // 9th
            50000 // 10th
        };

        public static void AssignHeighteningFromBEUs(Pawn pawn)
        {
            if (pawn?.story?.traits == null)
                return;

            var investedTrait = DefDatabase<TraitDef>.GetNamedSilentFail("Cosmere_Invested");
            if (investedTrait == null) return;

            var investNeed = pawn.needs?.TryGetNeed(DefDatabase<NeedDef>.GetNamed("Cosmere_Investiture"));
            if (investNeed == null) return;

            var degree = GetDegreeFromBEUs((int)investNeed.CurLevel);

            // Remove existing Invested trait (if present with different degree)
            var existing = pawn.story.traits.GetTrait(investedTrait);
            if (existing != null && existing.Degree != degree)
                pawn.story.traits.allTraits.Remove(existing);

            // Add trait if not already correct
            if (!pawn.story.traits.HasTrait(investedTrait))
                pawn.story.traits.GainTrait(new Trait(investedTrait, degree));
        }

        public static int GetDegreeFromBEUs(int beu)
        {
            for (var i = BEU_THRESHOLDS.Length - 1; i >= 0; i--)
                if (beu >= BEU_THRESHOLDS[i])
                    return i;
            return 0;
        }
    }
}