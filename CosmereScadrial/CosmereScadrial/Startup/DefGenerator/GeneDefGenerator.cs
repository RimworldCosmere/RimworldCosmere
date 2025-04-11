using System.Collections.Generic;
using System.Linq;
using CosmereScadrial;
using CosmereScadrial.DefModExtensions;
using CosmereScadrial.Defs;
using CosmereScadrial.Registry;
using Verse;

namespace Cosmere.Scadrial.Startup.DefGenerator
{
    [StaticConstructorOnStartup]
    public static class GeneDefGenerator
    {
        static GeneDefGenerator()
        {
            Log.Message("[CosmereScadrial] Starting GeneDefGenerator");

            var order = 3;
            foreach (var metal in MetalRegistry.Metals.Values.Where(metal => !metal.GodMetal))
            {
                if (metal.Allomancy != null && !string.IsNullOrEmpty(metal.Allomancy.UserName))
                    DefDatabase<GeneDef>.Add(CreateMistingGene(metal, order++));

                if (metal.Feruchemy != null && !string.IsNullOrEmpty(metal.Feruchemy.UserName))
                    DefDatabase<GeneDef>.Add(CreateFerringGene(metal, order++));
            }

            DefDatabase<GeneDef>.Add(CreateMistbornGene());
            DefDatabase<GeneDef>.Add(CreateFullFeruchemistGene());

            var addedDefs = DefDatabase<GeneDef>.AllDefs.Where(g =>
                g.defName.StartsWith("Cosmere_Misting_") || g.defName.StartsWith("Cosmere_Ferring_"));
            Log.Message(
                $"[CosmereScadrial] Generated {addedDefs.Count() + 1} GeneDefs. Click to see\n\n\n\t{string.Join("\n\t", addedDefs.Select(x => x.defName))}\n\n\n");
        }

        private static GeneCategoryDef GetCategory(bool isAllomancy)
        {
            return DefDatabase<GeneCategoryDef>.GetNamed(isAllomancy
                ? "Cosmere_Scadrial_Allomancy"
                : "Cosmere_Scadrial_Feruchemy");
        }

        private static GeneDef CreateMistingGene(MetalInfo metal, int order)
        {
            return new InvestitureBaseGene
            {
                defName = $"Cosmere_Misting_{metal.Name.CapitalizeFirst()}",
                label = metal.Name + " Misting",
                labelShortAdj = metal.Allomancy.UserName,
                iconPath = $"UI/Icons/Genes/Investiture/Allomancy/{metal.Name.CapitalizeFirst()}",
                description = metal.Allomancy.Description,
                displayCategory = GetCategory(true),
                displayOrderInCategory = order,
                modExtensions = new List<DefModExtension>
                    { new MetalLinked { metals = new List<string> { metal.Name } } }
            };
        }

        private static GeneDef CreateFerringGene(MetalInfo metal, int order)
        {
            return new InvestitureBaseGene
            {
                defName = $"Cosmere_Ferring_{metal.Name.CapitalizeFirst()}",
                label = metal.Name + " Ferring",
                labelShortAdj = metal.Feruchemy.UserName,
                iconPath = $"UI/Icons/Genes/Investiture/Feruchemy/{metal.Name.CapitalizeFirst()}",
                description = metal.Feruchemy.Description,
                displayCategory = GetCategory(false),
                displayOrderInCategory = order,
                modExtensions = new List<DefModExtension>
                    { new MetalLinked { metals = new List<string> { metal.Name } } }
            };
        }

        private static GeneDef CreateMistbornGene()
        {
            return new InvestitureBaseGene
            {
                defName = "Cosmere_Mistborn",
                label = "mistborn",
                iconPath = "UI/Icons/Genes/Investiture/Allomancy/Mistborn",
                description = "A rare individual who can burn all Allomantic metals.",
                displayCategory = GetCategory(true),
                displayOrderInCategory = 1,
                modExtensions =
                    new List<DefModExtension>
                    {
                        new MetalLinked
                        {
                            metals = MetalRegistry.Metals
                                .Where(m => m.Value.Allomancy != null && !m.Value.GodMetal)
                                .Select(m => m.Key)
                                .ToList()
                        }
                    }
            };
        }

        private static GeneDef CreateFullFeruchemistGene()
        {
            return new InvestitureBaseGene
            {
                defName = "Cosmere_FullFeruchemist",
                label = "full Feruchemist",
                iconPath = "UI/Icons/Genes/Investiture/Feruchemy/FullFeruchemist",
                description = "A rare individual who can use all Feruchemical metals.",
                displayCategory = GetCategory(false),
                displayOrderInCategory = 2,
                modExtensions = new List<DefModExtension>
                {
                    new MetalLinked
                    {
                        metals = MetalRegistry.Metals
                            .Where(m => m.Value.Feruchemy != null && !m.Value.GodMetal)
                            .Select(m => m.Key)
                            .ToList()
                    }
                }
            };
        }
    }

    public class InvestitureBaseGene : GeneDef
    {
        public InvestitureBaseGene()
        {
            geneClass = typeof(Gene_Metalborn);
            modContentPack =
                LoadedModManager.RunningMods.FirstOrDefault(m => m.PackageId == "cryptiklemur.cosmere.scadrial");
            passOnDirectly = false;
            biostatCpx = 5;
            biostatMet = 0;
            biostatArc = 1;
            exclusionTags = new List<string>();
            randomChosen = false;
        }
    }
}