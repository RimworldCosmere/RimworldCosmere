using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.Roshar;

//@todo Split into DefOf per type
[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class Defs {
    public static TraitDef Cosmere_Roshar_Trait_Radiant_Windrunner;
    public static TraitDef Cosmere_Roshar_Trait_Radiant_Truthwatcher;
    public static TraitDef Cosmere_Roshar_Trait_Radiant_Edgedancer;
    public static TraitDef Cosmere_Roshar_Trait_Radiant_Skybreaker;

    public static AbilityDef Cosmere_Roshar_SummonShardblade;
    public static AbilityDef Cosmere_Roshar_UnbondBlade;
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
    public static ThingDef Cosmere_Roshar_FabrialCage_Pewter;
    public static ThingDef Cosmere_Roshar_FabrialCage_Tin;
    public static ThingDef Cosmere_Roshar_BasicFabrial_Augmenter;

    // FABRIALS
    public static ThingDef Cosmere_Roshar_Apparel_Fabrial_Painrial_Diminisher;

    // WEAPONS
    public static ThingDef Cosmere_Roshar_MeleeWeapon_Shardblade;


    // HEDIFF
    public static HediffDef Cosmere_Roshar_Painrial_Augment;
    public static HediffDef Cosmere_Roshar_Painrial_Diminisher;
    public static HediffDef Cosmere_Roshar_Apparel_Painrial_Diminisher_Hediff;

    public static HediffDef Cosmere_Roshar_Logirial_Augment;
    public static HediffDef Cosmere_Roshar_Logirial_Diminisher;

    public static HediffDef Cosmere_Roshar_Surge_Abrasion;

    static Defs() {
        DefOfHelper.EnsureInitializedInCtor(typeof(Defs));
    }
}