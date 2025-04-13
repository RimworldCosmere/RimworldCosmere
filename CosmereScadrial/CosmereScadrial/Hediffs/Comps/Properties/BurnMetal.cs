using Verse;

namespace CosmereScadrial.Hediffs.Comps.Properties {
    public class BurnMetal : HediffCompProperties {
        public string metal;
        public int unitsPerBurn = 1;

        public BurnMetal() {
            compClass = typeof(Comps.BurnMetal);
        }
    }
}