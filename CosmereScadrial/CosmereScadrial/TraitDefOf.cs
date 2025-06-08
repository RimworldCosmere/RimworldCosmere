#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereScadrial {
    [DefOf]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public static partial class TraitDefOf {
        public static TraitDef Cosmere_Mistborn;
        public static TraitDef Cosmere_FullFeruchemist;
        public static TraitDef Cosmere_Metalborn;

        static TraitDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(TraitDefOf));
        }
    }
}