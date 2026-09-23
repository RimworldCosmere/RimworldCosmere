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
public static partial class TraitDefOf {
    [MayRequire("Cosmere.Scadrial")]
    public static TraitDef Cosmere_Scadrial_Trait_Metalborn;

    [MayRequire("Cosmere.Scadrial")]
    public static TraitDef Cosmere_Scadrial_Trait_Allomancer;

    [MayRequire("Cosmere.Scadrial")]
    public static TraitDef Cosmere_Scadrial_Trait_Feruchemist;

    [MayRequire("Cosmere.Scadrial")]
    public static TraitDef Cosmere_Scadrial_Trait_Mistborn;

    [MayRequire("Cosmere.Scadrial")]
    public static TraitDef Cosmere_Scadrial_Trait_FullFeruchemist;

    [MayRequire("Cosmere.Scadrial")]
    public static TraitDef Cosmere_Scadrial_Trait_Drab;

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
