using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereScadrial {
    [DefOf]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnassignedField.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public static class ThoughtDefOf {
        public static ThoughtDef Cosmere_Scadrial_Snapped;

        public static ThoughtDef Cosmere_Thought_Gold_Dread;
        public static ThoughtDef Cosmere_Thought_Gold_Awe;
        public static ThoughtDef Cosmere_Thought_Gold_Gratitude;
        public static ThoughtDef Cosmere_Thought_Gold_Regret;
        public static ThoughtDef Cosmere_Thought_Gold_Shame;
        public static ThoughtDef Cosmere_Thought_Gold_Rage;
        public static ThoughtDef Cosmere_Thought_Gold_Loss;
        public static ThoughtDef Cosmere_Thought_Gold_Hope;
        public static ThoughtDef Cosmere_Thought_Gold_Pride;
        public static ThoughtDef Cosmere_Thought_Gold_Determination;
        public static ThoughtDef Cosmere_Thought_Gold_Jealousy;
        public static ThoughtDef Cosmere_Thought_Gold_Curiosity;

        public static ThoughtDef Cosmere_Thought_PostGold_Afterglow;

        static ThoughtDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDefOf));
        }
    }
}