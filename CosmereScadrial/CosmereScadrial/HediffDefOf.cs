using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace CosmereScadrial {
    [DefOf]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public static class HediffDefOf {
        public static HediffDef Cosmere_Hediff_BrassAura;
        public static HediffDef Cosmere_Hediff_Soothed;
        public static HediffDef Cosmere_Hediff_BronzeAura;
        public static HediffDef Cosmere_Hediff_SteelAura;
        public static HediffDef Cosmere_Hediff_IronAura;
        public static HediffDef Cosmere_Hediff_ZincAura;
        public static HediffDef Cosmere_Hediff_Rioted_Buff;
        public static HediffDef Cosmere_Hediff_Rioted_Debuff;
        public static HediffDef Cosmere_Hediff_CopperClouded;
        public static HediffDef Cosmere_Hediff_CopperAura;
        public static HediffDef Cosmere_Hediff_PewterBuff;
        public static HediffDef Cosmere_Hediff_TinBuff;
        public static HediffDef Cosmere_Hediff_PewterDrag;
        public static HediffDef Cosmere_Hediff_TinDrag;
        public static HediffDef Cosmere_Scadrial_MistComa;

        static HediffDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
        }
    }
}