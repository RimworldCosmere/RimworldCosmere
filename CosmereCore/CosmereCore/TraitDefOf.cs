#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereCore {
    [DefOf]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public static class TraitDefOf {
        public static TraitDef Cosmere_Invested;

        static TraitDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(TraitDefOf));
        }
    }
}