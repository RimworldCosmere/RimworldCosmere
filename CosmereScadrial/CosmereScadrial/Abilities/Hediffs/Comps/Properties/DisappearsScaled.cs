using CosmereFramework.Utils;
using Verse;

namespace CosmereScadrial.Abilities.Hediffs.Comps.Properties {
    public class DisappearsScaled : HediffCompProperties {
        public int baseTicks = (int)TickUtility.Minutes(5); // 1 in-game hour

        public DisappearsScaled() {
            compClass = typeof(Comps.DisappearsScaled);
        }
    }
}