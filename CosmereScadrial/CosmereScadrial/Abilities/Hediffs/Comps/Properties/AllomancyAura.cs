using Verse;

namespace CosmereScadrial.Abilities.Hediffs.Comps.Properties {
    public class AllomancyAura : HediffCompProperties {
        public HediffDef actHediff;
        public string actVerb;
        public float radius = 18f;
        public int tickInterval = 60;

        public AllomancyAura() {
            compClass = typeof(Comps.AllomancyAura);
        }
    }
}