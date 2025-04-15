using RimWorld;
using Verse;

namespace CosmereScadrial.Comps.Properties {
    public class ToggleBurnMetal : CompProperties_AbilityEffect {
        public HediffDef hediff;

        public ToggleBurnMetal() {
            compClass = typeof(Comps.ToggleBurnMetal);
        }
    }
}