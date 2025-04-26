#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace CosmereScadrial {
    [DefOf]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public static class GeneDefOf {
        public static GeneDef Cosmere_Misting_Iron;
        public static GeneDef Cosmere_Ferring_Iron;
        public static GeneDef Cosmere_Misting_Steel;
        public static GeneDef Cosmere_Ferring_Steel;
        public static GeneDef Cosmere_Misting_Tin;
        public static GeneDef Cosmere_Ferring_Tin;
        public static GeneDef Cosmere_Misting_Pewter;
        public static GeneDef Cosmere_Ferring_Pewter;
        public static GeneDef Cosmere_Misting_Zinc;
        public static GeneDef Cosmere_Ferring_Zinc;
        public static GeneDef Cosmere_Misting_Brass;
        public static GeneDef Cosmere_Ferring_Brass;
        public static GeneDef Cosmere_Misting_Copper;
        public static GeneDef Cosmere_Ferring_Copper;
        public static GeneDef Cosmere_Misting_Bronze;
        public static GeneDef Cosmere_Ferring_Bronze;
        public static GeneDef Cosmere_Misting_Aluminum;
        public static GeneDef Cosmere_Ferring_Aluminum;
        public static GeneDef Cosmere_Misting_Duralumin;
        public static GeneDef Cosmere_Ferring_Duralumin;
        public static GeneDef Cosmere_Misting_Gold;
        public static GeneDef Cosmere_Ferring_Gold;

        public static GeneDef Cosmere_Mistborn;
        public static GeneDef Cosmere_FullFeruchemist;

        static GeneDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(GeneDefOf));
        }
    }
}