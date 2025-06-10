using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereCore {
    [DefOf]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public static class NeedDefOf {
        public static NeedDef Cosmere_Investiture;

        static NeedDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(NeedDefOf));
        }
    }
}