using System.Linq;
using CosmereScadrial.Defs;
using RimWorld;
using Verse;

namespace CosmereScadrial.Utils {
    public static class MetalbornUtility {
        public static bool HasAnyMetalbornGene(Pawn pawn) {
            if (pawn?.genes == null) return false;

            var allAllomancyGeneDefs = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading.Where(x => x.allomancy != null)
                .Select(metal => $"Cosmere_Misting_{metal.defName}");
            var allFeruchemyGeneDefs = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading.Where(x => x.feruchemy != null)
                .Select(metal => $"Cosmere_Ferring_{metal.defName}");

            return pawn.genes.GenesListForReading
                .Any(gene =>
                    allAllomancyGeneDefs.Contains(gene.def.defName)
                    || allFeruchemyGeneDefs.Contains(gene.def.defName)
                    || gene.def.Equals(GeneDefOf.Cosmere_Mistborn)
                    || gene.def.Equals(GeneDefOf.Cosmere_FullFeruchemist)
                );
        }

        public static void HandleMetalbornTrait(Pawn pawn) {
            if (!HasAnyMetalbornGene(pawn)) {
                var trait = pawn.story.traits.GetTrait(TraitDefOf.Cosmere_Metalborn);
                if (trait != null) pawn.story.traits.RemoveTrait(trait);
                return;
            }

            if (!pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Metalborn)) pawn.story.traits.GainTrait(new Trait(TraitDefOf.Cosmere_Metalborn));
        }

        public static void HandleAbilities(Pawn pawn, GeneDef def) {
            //pawn.TryGetComp<RightClickAbilities>()?.TryAddAllomanticAbilities(def);
        }
    }
}