using CosmereScadrial.CompProperties;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Comps {
    public class CompAllomanticVial : ThingComp {
        public CompProperties_AllomanticVial Props {
            get => (CompProperties_AllomanticVial)props;
        }

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Collections.Look(ref Props.metalNames, "metalNames", LookMode.Value);
        }

        public override void PostIngested(Pawn ingester) {
            base.PostIngested(ingester);

            foreach (var metal in Props.metalNames) {
                // Example hook to your investiture system
                AllomancyUtility.AddMetalReserve(ingester, metal, 100f); // or adjust amount
                Messages.Message($"{ingester.LabelShortCap} absorbed {metal}.", ingester, MessageTypeDefOf.PositiveEvent);
            }
        }

        public override string CompInspectStringExtra() {
            return $"Contains: {string.Join(", ", Props.metalNames)}";
        }
    }
}