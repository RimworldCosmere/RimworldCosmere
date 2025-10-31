using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Utility;

public static class Constants {
    public static List<TraitDef> radiantTraits => [];

    public static List<ThingDef> rawGems => [
        Core.ThingDefOf.RawDiamond,
        Core.ThingDefOf.RawGarnet,
        Core.ThingDefOf.RawRuby,
        Core.ThingDefOf.RawSapphire,
        Core.ThingDefOf.RawEmerald,
    ];

    public static List<ThingDef> cutGems => [
        Core.ThingDefOf.CutGem,
    ];
}