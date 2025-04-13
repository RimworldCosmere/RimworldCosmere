using CosmereCore.Needs;
using CosmereScadrial.Comps;
using CosmereScadrial.Registry;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Utils {
    public static class AllomancyUtility {
        /**
         * Based on a FULL msitborn burning every metal being capped around 5 BEUs
         * 5 BEU / 16 metals / 100 units per metal ends up with this conversion rate
         */
        public const float BEU_PER_METAL_UNIT = 0.003125f;

        public static void AddMetalReserve(Pawn pawn, string metal, float amount) {
            var tracker = pawn.GetComp<ScadrialInvestiture>();
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
            var tracker = pawn.GetComp<ScadrialInvestiture>();
            if (tracker == null || !tracker.metalReserves.TryGetValue(metal.ToLower(), out var value)) {
                return 0f;
            }

            var max = MetalRegistry.Metals[metal.ToLower()].MaxAmount;
            return value / max;
        }

        /// <summary>
        ///     Tries to burn the specified metal to generate the required BEUs of investiture.
        /// </summary>
        /// <param name="pawn">The pawn performing the burn.</param>
        /// <param name="metal">The metal to burn.</param>
        /// <param name="requiredBEU">The amount of Investiture to generate.</param>
        /// <returns>True if successful; false if not enough metal is available.</returns>
        public static bool TryBurnMetalForInvestiture(Pawn pawn, string metal, float requiredBEU) {
            var metalNeeded = requiredBEU / BEU_PER_METAL_UNIT;

            if (!pawn.TryGetComp<ScadrialInvestiture>(out var comp) ||
                !comp.metalReserves.TryGetValue(metal, out var available) ||
                !(available >= metalNeeded)) {
                return false;
            }

            var investitureNeed = pawn.needs.TryGetNeed<Need_Investiture>();
            if (investitureNeed == null) {
                return false;
            }

            comp.metalReserves[metal] -= metalNeeded;
            investitureNeed.CurLevel = Mathf.Min(
                investitureNeed.CurLevel + requiredBEU,
                investitureNeed.MaxLevel
            );

            return true;
        }

        public static float GetMetalNeededForBEU(float requiredBEU) {
            return requiredBEU / BEU_PER_METAL_UNIT;
        }
    }
}