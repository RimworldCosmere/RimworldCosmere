using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Utility;

public static class Constants {
    public static readonly List<TraitDef> radiantTraits = [];

    public static readonly List<ThingDef> rawGems = [
        Core.ThingDefOf.RawDiamond,
        Core.ThingDefOf.RawGarnet,
        Core.ThingDefOf.RawRuby,
        Core.ThingDefOf.RawSapphire,
        Core.ThingDefOf.RawEmerald,
    ];

    public static readonly List<ThingDef> cutGems = [
        Core.ThingDefOf.CutGem,
    ];
}