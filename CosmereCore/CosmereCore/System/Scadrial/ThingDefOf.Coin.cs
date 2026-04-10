using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static partial class ThingDefOf {
    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Thing_Boxing;

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Thing_Clip;

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Thing_ClipProjectile;
}