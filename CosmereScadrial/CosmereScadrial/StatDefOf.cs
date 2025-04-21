using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereScadrial {
    [DefOf]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public static class StatDefOf {
        public static StatDef Cosmere_Allomantic_Power;

        static StatDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(StatDefOf));
        }
    }
}