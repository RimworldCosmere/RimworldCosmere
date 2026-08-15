#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class KolossLifeStageDefOf {
    [MayRequire("Cosmere.Scadrial")]
    public static LifeStageDef Cosmere_Scadrial_LifeStage_KolossYoung;

    [MayRequire("Cosmere.Scadrial")]
    public static LifeStageDef Cosmere_Scadrial_LifeStage_KolossGrown;

    [MayRequire("Cosmere.Scadrial")]
    public static LifeStageDef Cosmere_Scadrial_LifeStage_KolossOvergrown;

    [MayRequire("Cosmere.Scadrial")]
    public static LifeStageDef Cosmere_Scadrial_LifeStage_KolossSplitting;
}
