using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereScadrial {
    [DefOf]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public static class ThoughtDefOf {
        public static ThoughtDef Cosmere_Scadrial_Snapped;

        static ThoughtDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDefOf));
        }
    }
}