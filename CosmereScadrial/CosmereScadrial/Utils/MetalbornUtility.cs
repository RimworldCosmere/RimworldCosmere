using System.Linq;
using CosmereScadrial.Registry;
using RimWorld;
using Verse;

namespace CosmereScadrial.Utils
{
    public static class MetalbornUtility
    {
        public static bool HasAnyMetalbornGene(Pawn pawn)
        {
            if (pawn?.genes == null) return false;

            var allAllomancyGeneDefs = MetalRegistry.Metals
                .Select(metal => $"Cosmere_Misting_{metal.Key.CapitalizeFirst()}");
            var allFeruchemyGeneDefs = MetalRegistry.Metals
                .Select(metal => $"Cosmere_Ferring_{metal.Key.CapitalizeFirst()}");

            return pawn.genes.GenesListForReading
                .Any(gene =>
                    allAllomancyGeneDefs.Contains(gene.def.defName)
                    || allFeruchemyGeneDefs.Contains(gene.def.defName)
                    || gene.def.defName == "Cosmere_Mistborn"
                    || gene.def.defName == "Cosmere_FullFeruchemist"
                );
        }

        public static void HandleMetalbornTrait(Pawn pawn)
        {
            var traitDef = TraitDef.Named("Cosmere_Metalborn");
            if (!HasAnyMetalbornGene(pawn))
            {
                var trait = pawn.story.traits.GetTrait(traitDef);
                if (trait != null) pawn.story.traits.RemoveTrait(trait);
                return;
            }

            if (!pawn.story.traits.HasTrait(traitDef)) pawn.story.traits.GainTrait(new Trait(traitDef));
        }
    }
}