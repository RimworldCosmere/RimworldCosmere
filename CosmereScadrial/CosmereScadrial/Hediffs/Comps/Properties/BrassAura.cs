using Verse;

namespace CosmereScadrial.Hediffs.Comps.Properties {
    public class BrassAura : HediffCompProperties {
        public float radius = 18f;
        public int tickInterval = 60;

        public BrassAura() {
            compClass = typeof(Comps.BrassAura);
        }
    }
}