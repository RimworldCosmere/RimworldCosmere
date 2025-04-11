using System.Collections.Generic;
using System.Linq;
using CosmereScadrial;
using CosmereScadrial.DefModExtensions;
using CosmereScadrial.Registry;
using RimWorld;
using Verse;

namespace Cosmere.Scadrial.Startup.DefGenerator {
    [StaticConstructorOnStartup]
    public static class TraitDefGenerator {
        static TraitDefGenerator() {
            Log.Message("[CosmereScadrial] Starting TraitDefGenerator");

            var order = 3;
            foreach (var metal in MetalRegistry.Metals.Values.Where(metal => !metal.GodMetal)) {
                if (metal.Allomancy != null && !string.IsNullOrEmpty(metal.Allomancy.UserName)) {
                    DefDatabase<TraitDef>.Add(CreateMistingTrait(metal, order++));
                }

                if (metal.Feruchemy != null && !string.IsNullOrEmpty(metal.Feruchemy.UserName)) {
                    DefDatabase<TraitDef>.Add(CreateFerringTrait(metal, order++));
                }
            }

            DefDatabase<TraitDef>.Add(CreateMistbornTrait());
            DefDatabase<TraitDef>.Add(CreateFullFeruchemistTrait());

            var addedDefs = DefDatabase<TraitDef>.AllDefs.Where(g =>
                g.defName.StartsWith("Cosmere_Misting_") || g.defName.StartsWith("Cosmere_Ferring_"));
            Log.Message(
                $"[CosmereScadrial] Generated {addedDefs.Count() + 1} TraitDefs. Click to see\n\n\n\t{string.Join("\n\t", addedDefs.Select(x => x.defName))}\n\n\n");
        }

        private static TraitDef CreateMistingTrait(MetalInfo metal, int order) =>
            new InvestitureBaseTrait {
                defName = $"Cosmere_Misting_{metal.Name.CapitalizeFirst()}",
                label = metal.Allomancy.UserName,
                description = metal.Allomancy.Description,
                modExtensions = new List<DefModExtension>
                    { new MetalLinked { metals = new List<string> { metal.Name } } },
                degreeDatas = new List<TraitDegreeData> {
                    CreateTrateDegreeData(metal, true),
                },
            };

        private static TraitDef CreateFerringTrait(MetalInfo metal, int order) =>
            new InvestitureBaseTrait {
                defName = $"Cosmere_Ferring_{metal.Name.CapitalizeFirst()}",
                label = metal.Feruchemy.UserName,
                description = metal.Feruchemy.Description,
                modExtensions = new List<DefModExtension>
                    { new MetalLinked { metals = new List<string> { metal.Name } } },
                degreeDatas = new List<TraitDegreeData> {
                    CreateTrateDegreeData(metal, false),
                },
            };

        private static TraitDef CreateMistbornTrait() {
            return new InvestitureBaseTrait {
                defName = "Cosmere_Mistborn",
                label = "Mistborn",
                description = "A rare individual who can burn all Allomantic metals.",
                modExtensions =
                    new List<DefModExtension> {
                        new MetalLinked {
                            metals = MetalRegistry.Metals
                                .Where(m => m.Value.Allomancy != null && !m.Value.GodMetal)
                                .Select(m => m.Key)
                                .ToList(),
                        },
                    },
                degreeDatas = new List<TraitDegreeData> {
                    new TraitDegreeData {
                        label = "Mistborn",
                        description = "A rare individual who can burn all Allomantic metals.",
                        degree = 0,
                        commonality = 0,
                    },
                },
            };
        }

        private static TraitDef CreateFullFeruchemistTrait() {
            return new InvestitureBaseTrait {
                defName = "Cosmere_FullFeruchemist",
                label = "Full Feruchemist",
                description = "A rare individual who can use all Feruchemical metals.",
                modExtensions = new List<DefModExtension> {
                    new MetalLinked {
                        metals = MetalRegistry.Metals
                            .Where(m => m.Value.Feruchemy != null && !m.Value.GodMetal)
                            .Select(m => m.Key)
                            .ToList(),
                    },
                },
                degreeDatas = new List<TraitDegreeData> {
                    new TraitDegreeData {
                        label = "Full Feruchemist",
                        description = "A rare individual who can use all Feruchemical metals.",
                        degree = 0,
                        commonality = 0,
                    },
                },
            };
        }

        private static TraitDegreeData CreateTrateDegreeData(MetalInfo metal, bool isAllomancy) =>
            new TraitDegreeData {
                label = isAllomancy ? metal.Allomancy.UserName : metal.Feruchemy.UserName,
                description = isAllomancy ? metal.Allomancy.Description : metal.Feruchemy.Description,
                degree = 0,
                commonality = 0,
            };
    }

    public class InvestitureBaseTrait : TraitDef {
        public InvestitureBaseTrait() => allowOnHostileSpawn = false;
    }
}