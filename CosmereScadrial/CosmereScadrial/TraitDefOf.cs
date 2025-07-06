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
public static partial class TraitDefOf {
    public static TraitDef Cosmere_Scadrial_Trait_Metalborn;
    public static TraitDef Cosmere_Scadrial_Trait_DormantMetalborn;
    public static TraitDef Cosmere_Scadrial_Trait_Allomancer;
    public static TraitDef Cosmere_Scadrial_Trait_Feruchemist;
    public static TraitDef Cosmere_Scadrial_Trait_Mistborn;
    public static TraitDef Cosmere_Scadrial_Trait_FullFeruchemist;

    static TraitDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(TraitDefOf));
    }

    public static TraitDef GetMistingTraitForMetal(MetalDef def) {
        return DefDatabase<TraitDef>.GetNamed("Cosmere_Scadrial_Trait_Misting" + def.defName, false);
    }


    public static TraitDef GetFerringTraitForMetal(MetalDef def) {
        return DefDatabase<TraitDef>.GetNamed("Cosmere_Scadrial_Trait_Ferring" + def.defName, false);
    }
}