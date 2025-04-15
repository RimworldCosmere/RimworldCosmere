using CosmereFramework.Utils;
using Verse;

namespace CosmereScadrial.Hediffs.Comps.Properties {
    public class BurnMetal : HediffCompProperties {
        public HediffDef dragHediff;
        public float unitsPerBurn = 0.1f / TickUtility.TicksPerSecond;

        public BurnMetal() {
            compClass = typeof(Comps.BurnMetal);
        }
    }
}