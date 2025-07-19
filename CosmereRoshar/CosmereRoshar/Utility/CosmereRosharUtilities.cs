using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CosmereRoshar.Utility;

public static class CosmereRosharUtilities {
    public static List<TraitDef> radiantTraits => [
        CosmereRosharDefs.Cosmere_Roshar_RadiantWindrunner,
        CosmereRosharDefs.Cosmere_Roshar_RadiantTruthwatcher,
        CosmereRosharDefs.Cosmere_Roshar_RadiantEdgedancer,
        CosmereRosharDefs.Cosmere_Roshar_RadiantSkybreaker,
    ];

    public static List<ThingDef> rawGems => [
        CosmereResources.ThingDefOf.RawDiamond,
        CosmereResources.ThingDefOf.RawGarnet,
        CosmereResources.ThingDefOf.RawRuby,
        CosmereResources.ThingDefOf.RawSapphire,
        CosmereResources.ThingDefOf.RawEmerald,
    ];

    public static List<ThingDef> cutGems => [
        CosmereResources.ThingDefOf.CutDiamond,
        CosmereResources.ThingDefOf.CutGarnet,
        CosmereResources.ThingDefOf.CutRuby,
        CosmereResources.ThingDefOf.CutSapphire,
        CosmereResources.ThingDefOf.CutEmerald,
    ];
}