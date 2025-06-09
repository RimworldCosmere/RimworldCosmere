using Verse;

namespace CosmereScadrial {
    public class CosmereScadrialSettings : ModSettings {
        public MistsFrequency mistsFrequency = MistsFrequency.Daily;

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref mistsFrequency, "mistsFrequency");
        }
    }

    public enum MistsFrequency {
        Daily,
        Weekly,
        Monthly,
    }
}