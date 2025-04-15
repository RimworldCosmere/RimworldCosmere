using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Hediffs {
    public class BurningMetal : HediffWithComps {
        public override bool ShouldRemove => false;

        public override bool Visible => Severity > 0f;

        public override void PreRemoved() {
            Log.Error("Hediff being removed! oh no.");
        }
    }
}