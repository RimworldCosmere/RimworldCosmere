using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Util;

public static class RosharGemConstants {
    public static readonly List<TraitDef> RadiantTraits = [];

    public static readonly List<ThingDef> RawGems = [
        Core.ThingDefOf.RawDiamond,
        Core.ThingDefOf.RawGarnet,
        Core.ThingDefOf.RawRuby,
        Core.ThingDefOf.RawSapphire,
        Core.ThingDefOf.RawEmerald,
    ];

    public static readonly List<ThingDef> CutGems = [
        Core.ThingDefOf.CutGem,
    ];
}
