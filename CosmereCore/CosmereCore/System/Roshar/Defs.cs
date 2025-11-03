using System;
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar;

//@todo Split into DefOf per type
[Obsolete("Move these to their own DefOfs")]
[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class Defs {
    //JobDef
    [MayRequire("Cosmere.Roshar")]
    public static JobDef Cosmere_Roshar_RefuelFabrial;

    [MayRequire("Cosmere.Roshar")]
    public static JobDef Cosmere_Roshar_RemoveFromFabrial;

    [MayRequire("Cosmere.Roshar")]
    public static JobDef Cosmere_Roshar_CaptureSpren;

    //ThingDef
    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_FabrialCage_Pewter;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_FabrialCage_Tin;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_BasicFabrial_Augmenter;

    // FABRIALS
    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Apparel_Fabrial_Painrial_Diminisher;


    // HEDIFF
    [MayRequire("Cosmere.Roshar")]
    public static HediffDef Cosmere_Roshar_Painrial_Augment;

    [MayRequire("Cosmere.Roshar")]
    public static HediffDef Cosmere_Roshar_Painrial_Diminisher;

    [MayRequire("Cosmere.Roshar")]
    public static HediffDef Cosmere_Roshar_Apparel_Painrial_Diminisher_Hediff;

    [MayRequire("Cosmere.Roshar")]
    public static HediffDef Cosmere_Roshar_Logirial_Augment;

    [MayRequire("Cosmere.Roshar")]
    public static HediffDef Cosmere_Roshar_Logirial_Diminisher;

    [MayRequire("Cosmere.Roshar")]
    public static HediffDef Cosmere_Roshar_Surge_Abrasion;

    static Defs() {
        DefOfHelper.EnsureInitializedInCtor(typeof(Defs));
    }
}