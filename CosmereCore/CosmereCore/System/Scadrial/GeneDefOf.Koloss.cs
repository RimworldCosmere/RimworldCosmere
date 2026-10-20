#nullable disable
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial;

public static partial class GeneDefOf {
    /// <summary>Heavy rather than tall. Everything under five years old.</summary>
    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_KolossBuild_Young;

    /// <summary>Shoulder and arm. Five years and up.</summary>
    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_KolossBuild_Mature;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_KolossScars_Low;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_KolossScars_Medium;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_KolossScars_Heavy;
}
