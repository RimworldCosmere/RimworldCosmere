using CosmereCore.Needs;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using UnityEngine;
using Verse;
using AllomanticVial = CosmereScadrial.Comps.Things.AllomanticVial;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Utils {
    public static class AllomancyUtility {
        /**
         * Based on a FULL mistborn burning every metal being capped around 5 BEUs
         * 5 BEU / 16 metals / 100 units per metal ends up with this conversion rate
         */
        public const float BEU_PER_METAL_UNIT = 0.003125f;

        public static void AddMetalReserve(Pawn pawn, MetallicArtsMetalDef metal, float amount) {
            pawn.GetComp<MetalReserves>().AddReserve(metal, amount);
        }

        public static bool CanUseMetal(Pawn pawn, MetallicArtsMetalDef metal) {
            var mistborn = DefDatabase<GeneDef>.GetNamed("Cosmere_Mistborn");
            var misting = DefDatabase<GeneDef>.GetNamed($"Cosmere_Misting_{metal.defName}");

            return pawn.genes.HasActiveGene(mistborn) || pawn.genes.HasActiveGene(misting);
        }

        public static bool IsMistborn(Pawn pawn) {
            return pawn.genes.HasActiveGene(DefDatabase<GeneDef>.GetNamed("Cosmere_Mistborn"));
        }

        public static bool IsMisting(Pawn pawn, MetallicArtsMetalDef metal) {
            return !IsMistborn(pawn) && pawn.genes.HasActiveGene(DefDatabase<GeneDef>.GetNamed($"Cosmere_Misting_{metal.defName}"));
        }

        public static float GetReservePercent(Pawn pawn, MetallicArtsMetalDef metal) {
            var comp = pawn.GetComp<MetalReserves>();
            if (comp == null || !comp.TryGetReserve(metal, out var value)) {
                return 0f;
            }

            return value / MetalReserves.MAX_AMOUNT;
        }

        /// <summary>
        ///     Tries to burn the specified metal to generate the required BEUs of investiture.
        /// </summary>
        /// <param name="pawn">The pawn performing the burn.</param>
        /// <param name="metal">The metal to burn.</param>
        /// <param name="requiredBEUs">The amount of Investiture to generate.</param>
        /// <returns>True if successful; false if not enough metal is available.</returns>
        public static bool TryBurnMetalForInvestiture(Pawn pawn, MetallicArtsMetalDef metal, float requiredBEUs) {
            var metalNeeded = GetMetalNeededForBeu(requiredBEUs);
            var comp = pawn.GetComp<MetalReserves>();

            if (!comp.CanLowerReserve(metal, metalNeeded)) {
                return false;
            }

            comp.LowerReserve(metal, metalNeeded);

            var investitureNeed = pawn.needs.TryGetNeed<Investiture>();
            if (investitureNeed == null) throw new MissingReferenceException("Pawn is missing the Investiture need.");

            investitureNeed.CurLevel = Mathf.Min(
                investitureNeed.CurLevel + requiredBEUs,
                investitureNeed.MaxLevel
            );

            return true;
        }

        public static float GetMetalNeededForBeu(float requiredBeu) {
            return requiredBeu / BEU_PER_METAL_UNIT;
        }

        public static bool PawnConsumeVialWithMetal(Pawn pawn, MetallicArtsMetalDef metal, bool allowMultiVial = false) {
            if (pawn?.inventory?.innerContainer == null) return false;

            foreach (var vial in pawn.inventory.innerContainer) {
                var comp = vial.TryGetComp<AllomanticVial>();
                if (comp == null) continue;
                var metals = comp.props.metals;

                if (metals.Count > 1 && !allowMultiVial) continue;

                if (!metals.Contains(metal) || vial.stackCount <= 0) continue;

                vial.Ingested(pawn, 1f);
                Log.Warning($"{pawn.NameShortColored} is auto-ingesting {vial.LabelShort} for {metal}.");
                return true;
            }

            return false;
        }

        public static bool PawnHasVialForMetal(Pawn pawn, MetallicArtsMetalDef metal, bool allowMultiVial = false) {
            if (pawn?.inventory?.innerContainer == null) return false;

            foreach (var vial in pawn.inventory.innerContainer) {
                var comp = vial.TryGetComp<AllomanticVial>();
                if (comp == null) continue;
                var metals = comp.props.metals;

                if (metals.Count > 1 && !allowMultiVial) continue;

                if (metals.Contains(metal) && vial.stackCount > 0) return true;
            }

            return false;
        }
    }
}