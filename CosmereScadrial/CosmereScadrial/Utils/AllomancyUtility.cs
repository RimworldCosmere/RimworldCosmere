using CosmereScadrial.Comps;
using CosmereScadrial.Registry;
using Verse;

namespace CosmereScadrial.Utils {
    public static class AllomancyUtility {
        public static void AddMetalReserve(Pawn pawn, string metal, float amount) {
            var tracker = pawn.GetComp<CompScadrialInvestiture>();
            if (tracker == null) {
                Log.Warning($"{pawn.NameShortColored} has no InvestitureTracker. Skipping {metal} absorption.");
                return;
            }

            tracker.AddReserve(metal, amount);
        }

        public static bool CanUseMetal(Pawn pawn, string metal) {
            var mistborn = DefDatabase<GeneDef>.GetNamed("Cosmere_Mistborn");
            var misting = DefDatabase<GeneDef>.GetNamed($"Cosmere_Misting_{metal.CapitalizeFirst()}");

            return pawn.genes.HasActiveGene(mistborn) || pawn.genes.HasActiveGene(misting);
        }

        public static bool IsMistborn(Pawn pawn) {
            return pawn.genes.HasActiveGene(DefDatabase<GeneDef>.GetNamed("Cosmere_Mistborn"));
        }

        public static bool IsMisting(Pawn pawn, string metal) {
            return pawn.genes.HasActiveGene(DefDatabase<GeneDef>.GetNamed($"Cosmere_Misting_{metal.CapitalizeFirst()}"));
        }

        public static float GetReservePercent(Pawn pawn, string metal) {
            var tracker = pawn.GetComp<CompScadrialInvestiture>();
            if (tracker == null || !tracker.metalReserves.TryGetValue(metal, out var value)) {
                return 0f;
            }

            var max = MetalRegistry.Metals[metal.ToLower()].MaxAmount;
            return value / max;
        }
    }
}