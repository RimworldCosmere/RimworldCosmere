#nullable disable
using System.Diagnostics.CodeAnalysis;
using CosmereMetals.Defs;
using RimWorld;

namespace CosmereMetals {
    [DefOf]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public static class MetalDefOf {        
        public static MetalDef Iron;
        public static MetalDef Steel;
        public static MetalDef Tin;
        public static MetalDef Pewter;
        public static MetalDef Zinc;
        public static MetalDef Brass;
        public static MetalDef Copper;
        public static MetalDef Bronze;
        public static MetalDef Aluminum;
        public static MetalDef Duralumin;
        public static MetalDef Chromium;
        public static MetalDef Nicrosil;
        public static MetalDef Cadmium;
        public static MetalDef Bendalloy;
        public static MetalDef Gold;
        public static MetalDef Silver;
        public static MetalDef Nickel;
        public static MetalDef Electrum;
        public static MetalDef Lerasium;
        public static MetalDef Atium;
        public static MetalDef Harmonium;
        public static MetalDef Trellium;

        static MetalDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(MetalDefOf));
        }
    }
}