using Verse;

namespace CosmereScadrial.Hediffs.Comps.Properties {
    public class BurnMetal : HediffCompProperties {
        public string metal;
        public float unitsPerBurn = 0.3f;

        public BurnMetal() {
            compClass = typeof(Comps.BurnMetal);
        }
    }
}