using Verse;

namespace CosmereScadrial.Hediffs.Comps.Properties {
    public class BurnMetal : HediffCompProperties {
        public string metal;
        public int metalUnitsPerHour = 10;

        public BurnMetal() {
            compClass = typeof(Comps.BurnMetal);
        }
    }
}