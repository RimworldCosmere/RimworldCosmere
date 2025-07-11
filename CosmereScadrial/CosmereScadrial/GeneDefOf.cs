#nullable disable
using System.Diagnostics.CodeAnalysis;
using CosmereResources.Def;
using RimWorld;
using Verse;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static partial class GeneDefOf {
    public static GeneDef Cosmere_Scadrial_Gene_DormantMetalborn;

    // Genes for custom races
    public static GeneDef Cosmere_Scadrial_Gene_NobleHeritage;
    public static GeneDef Cosmere_Scadrial_Gene_NobleHighSocial;
    public static GeneDef Cosmere_Scadrial_Gene_NobleFastLearner;
    public static GeneDef Cosmere_Scadrial_Gene_NobleLowFertility;
    public static GeneDef Cosmere_Scadrial_Gene_NobleLowWorkDrive;

    public static GeneDef Cosmere_Scadrial_Gene_SkaaHeritage;
    public static GeneDef Cosmere_Scadrial_Gene_SkaaLowSocial;
    public static GeneDef Cosmere_Scadrial_Gene_SkaaLowLearning;
    public static GeneDef Cosmere_Scadrial_Gene_SkaaHighFertility;
    public static GeneDef Cosmere_Scadrial_Gene_SkaaEndurance;
    public static GeneDef Cosmere_Scadrial_Gene_SkaaPurity;
    public static GeneDef Cosmere_Scadrial_Gene_SkaaAgnostic;

    public static GeneDef Cosmere_Scadrial_Gene_TerrisHeritage;
    public static GeneDef Cosmere_Scadrial_Gene_TerrisDeepMemory;
    public static GeneDef Cosmere_Scadrial_Gene_TerrisPeaceful;
    public static GeneDef Cosmere_Scadrial_Gene_TerrisStability;

    public static GeneDef Cosmere_Scadrial_Gene_ScadrianHeritage;

    static GeneDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(GeneDefOf));
    }

    public static GeneDef GetMistingGeneForMetal(MetalDef def) {
        return DefDatabase<GeneDef>.GetNamed("Cosmere_Scadrial_Gene_Misting" + def.defName, false);
    }


    public static GeneDef GetFerringGeneForMetal(MetalDef def) {
        return DefDatabase<GeneDef>.GetNamed("Cosmere_Scadrial_Gene_Ferring" + def.defName, false);
    }
}