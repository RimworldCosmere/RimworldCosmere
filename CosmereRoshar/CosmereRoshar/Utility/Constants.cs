using RimWorld;
using Verse;

namespace Cosmere.Roshar.Utility;

public static class Constants {
    public static List<TraitDef> radiantTraits => [];

    public static List<ThingDef> rawGems => [
        Resources.ThingDefOf.RawDiamond,
        Resources.ThingDefOf.RawGarnet,
        Resources.ThingDefOf.RawRuby,
        Resources.ThingDefOf.RawSapphire,
        Resources.ThingDefOf.RawEmerald,
    ];

    public static List<ThingDef> cutGems => [
        Resources.ThingDefOf.CutGem,
    ];
}