using Verse;

namespace CosmereScadrial.Hediffs.Comps.Properties {
    public class DisappearsScaled : HediffCompProperties {
        public int baseTicks = 60000; // 1 in-game hour

        public DisappearsScaled() {
            compClass = typeof(Comps.DisappearsScaled);
        }
    }
}