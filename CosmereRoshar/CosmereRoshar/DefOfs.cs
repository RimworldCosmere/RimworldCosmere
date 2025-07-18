using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CosmereRoshar;

[DefOf]
public static class CosmereRosharDefs {
    public static NeedDef WhtwlRadiantProgress;

    public static TraitDef WhtwlRadiantWindrunner;
    public static TraitDef WhtwlRadiantTruthwatcher;
    public static TraitDef WhtwlRadiantEdgedancer;
    public static TraitDef WhtwlRadiantSkybreaker;

    public static AbilityDef WhtwlSummonShardblade;
    public static AbilityDef WhtwlUnbondBlade;
    public static AbilityDef WhtwlBreathStormlight;
    public static AbilityDef WhtwlSurgeOfHealing;
    public static AbilityDef WhtwlSurgeOfGrowth;
    public static AbilityDef WhtwlSurgeOfAbrasion;
    public static AbilityDef WhtwlSurgeOfDivision;
    public static AbilityDef WhtwlLashingUpward;
    public static AbilityDef WhtwlWindRunnerFlight;

    //JobDef
    public static JobDef WhtwlRefuelSphereLamp;
    public static JobDef WhtwlRefuelFabrial;
    public static JobDef WhtwlRemoveFromFabrial;
    public static JobDef WhtwlCastAbilityOnTarget;

    //ThingDef
    public static ThingDef WhtwlApparelSpherePouch;
    public static ThingDef WhtwlSphereLampWall;
    public static ThingDef WhtwlFabrialCagePewter;
    public static ThingDef WhtwlBasicFabrialAugmenter;

    // FABRIALS
    public static ThingDef WhtwlApparelFabrialPainrialDiminisher;

    // WEAPONS
    public static ThingDef WhtwlMeleeWeaponShardblade;


    // HEDIFF
    public static HediffDef WhtwlPainrialAgument;
    public static HediffDef WhtwlPainrialDiminisher;
    public static HediffDef WhtwlApparelPainrialDiminisherHediff;

    public static HediffDef WhtwlLogirialAgument;
    public static HediffDef WhtwlLogirialDiminisher;

    public static HediffDef WhtwlSurgeAbrasion;


    static CosmereRosharDefs() {
        DefOfHelper.EnsureInitializedInCtor(typeof(CosmereRosharDefs));
    }
}

public static class CosmereRosharUtilities {
    public static List<TraitDef> radiantTraits =>
        new List<TraitDef> {
            CosmereRosharDefs.WhtwlRadiantWindrunner,
            CosmereRosharDefs.WhtwlRadiantTruthwatcher,
            CosmereRosharDefs.WhtwlRadiantEdgedancer,
            CosmereRosharDefs.WhtwlRadiantSkybreaker,
        };

    public static List<ThingDef> rawGems =>
        new List<ThingDef> {
            CosmereResources.ThingDefOf.RawDiamond,
            CosmereResources.ThingDefOf.RawGarnet,
            CosmereResources.ThingDefOf.RawRuby,
            CosmereResources.ThingDefOf.RawSapphire,
            CosmereResources.ThingDefOf.RawEmerald,
        };

    public static List<ThingDef> cutGems =>
        new List<ThingDef> {
            CosmereResources.ThingDefOf.CutDiamond,
            CosmereResources.ThingDefOf.CutGarnet,
            CosmereResources.ThingDefOf.CutRuby,
            CosmereResources.ThingDefOf.CutSapphire,
            CosmereResources.ThingDefOf.CutEmerald,
        };
}