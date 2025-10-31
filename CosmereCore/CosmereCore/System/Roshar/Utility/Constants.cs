using Cosmere;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Utility;

public static class Constants {
    public static List<TraitDef> radiantTraits => [];

    public static List<ThingDef> rawGems => [
        Cosmere.ThingDefOf.RawDiamond,
        Cosmere.ThingDefOf.RawGarnet,
        Cosmere.ThingDefOf.RawRuby,
        Cosmere.ThingDefOf.RawSapphire,
        Cosmere.ThingDefOf.RawEmerald,
    ];

    public static List<ThingDef> cutGems => [
        Cosmere.ThingDefOf.CutGem,
    ];
}