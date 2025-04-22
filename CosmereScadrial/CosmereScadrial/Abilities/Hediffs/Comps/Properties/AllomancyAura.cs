using Verse;

namespace CosmereScadrial.Abilities.Hediffs.Comps.Properties {
    public class AllomancyAura : HediffCompProperties {
        public HediffDef actHediff;
        public string actVerb;
        public float radius = 18f;

        public AllomancyAura() {
            compClass = typeof(Comps.AllomancyAura);
        }
    }
}