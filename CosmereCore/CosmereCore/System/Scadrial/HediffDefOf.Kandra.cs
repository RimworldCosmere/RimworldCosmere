#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public static partial class HediffDefOf {
    [MayRequire("Cosmere.Scadrial")]
    public static HediffDef Cosmere_Scadrial_Hediff_BlessingOfPresence;

    [MayRequire("Cosmere.Scadrial")]
    public static HediffDef Cosmere_Scadrial_Hediff_BlessingOfPotency;

    [MayRequire("Cosmere.Scadrial")]
    public static HediffDef Cosmere_Scadrial_Hediff_BlessingOfStability;

    [MayRequire("Cosmere.Scadrial")]
    public static HediffDef Cosmere_Scadrial_Hediff_BlessingOfAwareness;

    [MayRequire("Cosmere.Scadrial")]
    public static HediffDef Cosmere_Scadrial_Hediff_Mistwraith;
}
