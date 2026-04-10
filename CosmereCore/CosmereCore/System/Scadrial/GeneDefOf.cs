#nullable disable
using System.Diagnostics.CodeAnalysis;
using Cosmere.Core.Def;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static partial class GeneDefOf {
    static GeneDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(GeneDefOf));
    }

    // Genes for custom races
    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_NobleHeritage;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_NobleHighSocial;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_NobleFastLearner;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_NobleLowFertility;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_NobleLowWorkDrive;


    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_SkaaHeritage;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_SkaaLowSocial;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_SkaaLowLearning;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_SkaaHighFertility;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_SkaaEndurance;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_SkaaPurity;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_SkaaAgnostic;


    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_TerrisHeritage;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_TerrisDeepMemory;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_TerrisPeaceful;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_TerrisStability;

    [MayRequire("Cosmere.Scadrial")]
    public static GeneDef Cosmere_Scadrial_Gene_ScadrianHeritage;

    public static GeneDef GetMistingGeneForMetal(MetalDef def) {
        return DefDatabase<GeneDef>.GetNamed("Cosmere_Scadrial_Gene_Misting" + def.defName, false);
    }


    public static GeneDef GetFerringGeneForMetal(MetalDef def) {
        return DefDatabase<GeneDef>.GetNamed("Cosmere_Scadrial_Gene_Ferring" + def.defName, false);
    }
}