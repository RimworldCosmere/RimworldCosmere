#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class HediffDefOf {
    static HediffDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
    }

    [MayRequire("Cosmere.Roshar")]
    public static HediffDef Cosmere_Roshar_Hediff_ShardbladeSummoning;

    [MayRequire("Cosmere.Roshar")]
    public static HediffDef Cosmere_Roshar_Hediff_StrainedBond;

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
}