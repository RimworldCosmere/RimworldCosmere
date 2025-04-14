using CosmereFramework.Utils;
using Verse;

namespace CosmereScadrial.Hediffs.Comps.Properties {
    public class BurnMetal : HediffCompProperties {
        public int burnInterval = (int)TickUtility.Seconds(1);
        public string metal;
        public int tickInterval = (int)TickUtility.Milliseconds(500);
        public float unitsPerBurn = 0.1f;

        public BurnMetal() {
            compClass = typeof(Comps.BurnMetal);
        }
    }
}