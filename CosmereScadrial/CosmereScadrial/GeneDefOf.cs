#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace CosmereScadrial {
    [DefOf]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public static partial class GeneDefOf {
        public static GeneDef Cosmere_Scadrial_DormantMetalborn;
        public static GeneDef Cosmere_Mistborn;
        public static GeneDef Cosmere_FullFeruchemist;
        
        static GeneDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(GeneDefOf));
        }
    }
}