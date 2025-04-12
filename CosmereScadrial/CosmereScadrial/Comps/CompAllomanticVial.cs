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
            if (Props.metalNames.Count > 1) {
                if (!AllomancyUtility.IsMistborn(ingester) || !Props.metalNames.Any(metal => AllomancyUtility.GetReservePercent(ingester, metal) < 0.8f)) {
                    return;
                }
            }

            var metal = Props.metalNames[0];
            if (!AllomancyUtility.CanUseMetal(ingester, metal) || AllomancyUtility.GetReservePercent(ingester, metal) > 0.8f) {
                return;
            }

            Props.metalNames.ForEach(metal => {
                // Todo, add Investiture
                AllomancyUtility.AddMetalReserve(ingester, metal, 100f); // or adjust amount
                Messages.Message($"{ingester.LabelShortCap} absorbed {metal}.", ingester, MessageTypeDefOf.PositiveEvent);
            });
        }

        public override string CompInspectStringExtra() {
            return $"Contains: {string.Join(", ", Props.metalNames)}";
        }
    }
}