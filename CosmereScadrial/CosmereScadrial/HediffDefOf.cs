using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace CosmereScadrial {
    [DefOf]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public static class HediffDefOf {
        public static HediffDef Cosmere_Hediff_Clouded;
        public static HediffDef Cosmere_Hediff_CopperAura;
        public static HediffDef Cosmere_Hediff_PewterBuff;
        public static HediffDef Cosmere_Hediff_PewterDrag;
        public static HediffDef Cosmere_Scadrial_MistComa;

        static HediffDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
        }
    }
}