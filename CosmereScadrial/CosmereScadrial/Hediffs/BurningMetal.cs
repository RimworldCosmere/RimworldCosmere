using Verse;

namespace CosmereScadrial.Hediffs {
    public class BurningMetal : HediffWithComps {
        public override string LabelBase {
            get {
                Log.Warning($"[CosmereScadrial] LabelBase: {base.LabelBase} Severity={Severity} Flaring={Severity >= 1f}");
                if (Severity >= 1f) {
                    return base.LabelBase + " (flaring)";
                }

                return base.LabelBase;
            }
        }
    }
}