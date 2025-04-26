#nullable disable
using System.Diagnostics.CodeAnalysis;
using CosmereScadrial.Defs;
using RimWorld;

namespace CosmereScadrial {
    [DefOf]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public static class MetallicArtsMetalDefOf {
        public static MetallicArtsMetalDef Iron;
        public static MetallicArtsMetalDef Steel;
        public static MetallicArtsMetalDef Tin;
        public static MetallicArtsMetalDef Pewter;
        public static MetallicArtsMetalDef Zinc;
        public static MetallicArtsMetalDef Brass;
        public static MetallicArtsMetalDef Copper;
        public static MetallicArtsMetalDef Bronze;
        public static MetallicArtsMetalDef Aluminum;
        public static MetallicArtsMetalDef Duralumin;
        public static MetallicArtsMetalDef Gold;
        public static MetallicArtsMetalDef Silver;
        public static MetallicArtsMetalDef Nickel;

        static MetallicArtsMetalDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(MetallicArtsMetalDefOf));
        }
    }
}