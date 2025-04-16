using Verse;

namespace CosmereScadrial.Hediffs.Comps.Properties {
    public class AllomancyTarget : HediffCompProperties {
        public HediffDef actHediff;
        public string actVerb;
        public float severityMultiplier;
        public int tickInterval = 60;

        public AllomancyTarget() {
            compClass = typeof(Comps.AllomancyTarget);
        }
    }
}