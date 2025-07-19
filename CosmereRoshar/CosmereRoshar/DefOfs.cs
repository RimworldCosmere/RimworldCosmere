using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace CosmereRoshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class CosmereRosharDefs {
    public static NeedDef Cosmere_Roshar_Need_RadiantProgress;

    public static TraitDef Cosmere_Roshar_Trait_RadiantWindrunner;
    public static TraitDef Cosmere_Roshar_Trait_RadiantTruthwatcher;
    public static TraitDef Cosmere_Roshar_Trait_RadiantEdgedancer;
    public static TraitDef Cosmere_Roshar_Trait_RadiantSkybreaker;

    public static AbilityDef Cosmere_Roshar_SummonShardblade;
    public static AbilityDef Cosmere_Roshar_UnbondBlade;
    public static AbilityDef Cosmere_Roshar_BreathStormlight;
    public static AbilityDef Cosmere_Roshar_SurgeOfHealing;
    public static AbilityDef Cosmere_Roshar_SurgeOfGrowth;
    public static AbilityDef Cosmere_Roshar_SurgeOfAbrasion;
    public static AbilityDef Cosmere_Roshar_SurgeOfDivision;
    public static AbilityDef Cosmere_Roshar_LashingUpward;
    public static AbilityDef Cosmere_Roshar_WindRunnerFlight;

    //JobDef
    public static JobDef Cosmere_Roshar_RefuelSphereLamp;
    public static JobDef Cosmere_Roshar_RefuelFabrial;
    public static JobDef Cosmere_Roshar_RemoveFromFabrial;
    public static JobDef Cosmere_Roshar_CastAbilityOnTarget;

    //ThingDef
    public static ThingDef Cosmere_Roshar_ApparelSpherePouch;
    public static ThingDef Cosmere_Roshar_SphereLampWall;
    public static ThingDef Cosmere_Roshar_FabrialCagePewter;
    public static ThingDef Cosmere_Roshar_BasicFabrialAugmenter;

    // FABRIALS
    public static ThingDef Cosmere_Roshar_ApparelFabrialPainrialDiminisher;

    // WEAPONS
    public static ThingDef Cosmere_Roshar_MeleeWeaponShardblade;


    // HEDIFF
    public static HediffDef Cosmere_Roshar_PainrialAugment;
    public static HediffDef Cosmere_Roshar_PainrialDiminisher;
    public static HediffDef Cosmere_Roshar_ApparelPainrialDiminisherHediff;

    public static HediffDef Cosmere_Roshar_LogirialAugment;
    public static HediffDef Cosmere_Roshar_LogirialDiminisher;

    public static HediffDef Cosmere_Roshar_SurgeAbrasion;

    static CosmereRosharDefs() {
        DefOfHelper.EnsureInitializedInCtor(typeof(CosmereRosharDefs));
    }
}