using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CosmereRoshar.Utility;

public static class CosmereRosharUtilities {
    public static List<TraitDef> radiantTraits => [
        CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantWindrunner,
        CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantTruthwatcher,
        CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantEdgedancer,
        CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantSkybreaker,
    ];

    public static List<ThingDef> rawGems => [
        CosmereResources.ThingDefOf.RawDiamond,
        CosmereResources.ThingDefOf.RawGarnet,
        CosmereResources.ThingDefOf.RawRuby,
        CosmereResources.ThingDefOf.RawSapphire,
        CosmereResources.ThingDefOf.RawEmerald,
    ];

    public static List<ThingDef> cutGems => [
        CosmereResources.ThingDefOf.CutGem,
    ];
}