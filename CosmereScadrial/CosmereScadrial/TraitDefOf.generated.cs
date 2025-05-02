#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereScadrial {
    [DefOf]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public static class TraitDefOf {
        public static TraitDef Cosmere_Misting_Iron;
        public static TraitDef Cosmere_Ferring_Iron;
        public static TraitDef Cosmere_Misting_Steel;
        public static TraitDef Cosmere_Ferring_Steel;
        public static TraitDef Cosmere_Misting_Tin;
        public static TraitDef Cosmere_Ferring_Tin;
        public static TraitDef Cosmere_Misting_Pewter;
        public static TraitDef Cosmere_Ferring_Pewter;
        public static TraitDef Cosmere_Misting_Zinc;
        public static TraitDef Cosmere_Ferring_Zinc;
        public static TraitDef Cosmere_Misting_Brass;
        public static TraitDef Cosmere_Ferring_Brass;
        public static TraitDef Cosmere_Misting_Copper;
        public static TraitDef Cosmere_Ferring_Copper;
        public static TraitDef Cosmere_Misting_Bronze;
        public static TraitDef Cosmere_Ferring_Bronze;
        public static TraitDef Cosmere_Misting_Aluminum;
        public static TraitDef Cosmere_Ferring_Aluminum;
        public static TraitDef Cosmere_Misting_Duralumin;
        public static TraitDef Cosmere_Ferring_Duralumin;
        public static TraitDef Cosmere_Misting_Chromium;
        public static TraitDef Cosmere_Ferring_Chromium;
        public static TraitDef Cosmere_Misting_Nicrosil;
        public static TraitDef Cosmere_Ferring_Nicrosil;
        public static TraitDef Cosmere_Misting_Cadmium;
        public static TraitDef Cosmere_Ferring_Cadmium;
        public static TraitDef Cosmere_Misting_Bendalloy;
        public static TraitDef Cosmere_Ferring_Bendalloy;
        public static TraitDef Cosmere_Misting_Gold;
        public static TraitDef Cosmere_Ferring_Gold;
        public static TraitDef Cosmere_Misting_Electrum;
        public static TraitDef Cosmere_Ferring_Electrum;

        public static TraitDef Cosmere_Mistborn;
        public static TraitDef Cosmere_FullFeruchemist;
        public static TraitDef Cosmere_Metalborn;

        static TraitDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(TraitDefOf));
        }
    }
}