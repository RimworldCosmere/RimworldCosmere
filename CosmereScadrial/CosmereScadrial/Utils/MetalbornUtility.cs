using System.Linq;
using CosmereScadrial.Defs;
using CosmereScadrial.Genes;
using RimWorld;
using Verse;

namespace CosmereScadrial.Utils {
    public static class MetalbornUtility {
        public static bool HasAnyMetalbornGene(Pawn pawn) {
            if (pawn?.genes == null) return false;

            var allAllomancyGeneDefs = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading
                .Where(x => x.allomancy != null)
                .Select(metal => $"Cosmere_Misting_{metal.defName}");
            var allFeruchemyGeneDefs = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading
                .Where(x => x.feruchemy != null)
                .Select(metal => $"Cosmere_Ferring_{metal.defName}");

            return pawn.genes.GenesListForReading.Any(gene => gene is Metalborn && gene.Active);
        }

        public static void HandleMetalbornTrait(Pawn pawn) {
            if (!HasAnyMetalbornGene(pawn)) {
                var trait = pawn.story.traits.GetTrait(TraitDefOf.Cosmere_Metalborn);
                if (trait != null) pawn.story.traits.RemoveTrait(trait);
                return;
            }

            if (!pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Metalborn)) {
                pawn.story.traits.GainTrait(new Trait(TraitDefOf.Cosmere_Metalborn));
            }
        }

        public static void HandleBurningMetalHediff(Pawn pawn) {
            var hediffDef = DefDatabase<HediffDef>.GetNamed("Cosmere_Scadrial_Burning_Metals");
            if (!HasAnyMetalbornGene(pawn)) {
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (hediff != null) pawn.health.RemoveHediff(hediff);
                return;
            }

            pawn.health.GetOrAddHediff(hediffDef);
        }
    }
}