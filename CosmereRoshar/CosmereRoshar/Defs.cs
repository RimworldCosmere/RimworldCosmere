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
    //JobDef
    public static JobDef Cosmere_Roshar_RefuelSphereLamp;
    public static JobDef Cosmere_Roshar_RefuelFabrial;
    public static JobDef Cosmere_Roshar_RemoveFromFabrial;
    public static JobDef Cosmere_Roshar_CaptureSpren;

    //ThingDef
    public static ThingDef Cosmere_Roshar_FabrialCage_Pewter;
    public static ThingDef Cosmere_Roshar_FabrialCage_Tin;
    public static ThingDef Cosmere_Roshar_BasicFabrial_Augmenter;

    // FABRIALS
    public static ThingDef Cosmere_Roshar_Apparel_Fabrial_Painrial_Diminisher;


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