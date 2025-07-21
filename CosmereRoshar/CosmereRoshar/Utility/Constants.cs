using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.Roshar.Utility;

public static class Constants {
    public static List<TraitDef> radiantTraits => [
        Defs.Cosmere_Roshar_Trait_Radiant_Windrunner,
        Defs.Cosmere_Roshar_Trait_Radiant_Truthwatcher,
        Defs.Cosmere_Roshar_Trait_Radiant_Edgedancer,
        Defs.Cosmere_Roshar_Trait_Radiant_Skybreaker,
    ];

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