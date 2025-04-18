using CosmereFramework.Utils;
using Verse;

namespace CosmereScadrial.Abilities.Hediffs.Comps.Properties {
    public class DisappearsScaled : HediffCompProperties {
        public int baseTicks = (int)TickUtility.Minutes(30); // 1 in-game hour
        public float minSeverity = 0f;
        public bool scaleSeverity = true;

        public DisappearsScaled() {
            compClass = typeof(Comps.DisappearsScaled);
        }
    }
}