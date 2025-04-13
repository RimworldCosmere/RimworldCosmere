using System.Linq;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Comps {
    public class AllomanticVial : ThingComp {
        public Properties.AllomanticVial Props {
            get => (Properties.AllomanticVial)props;
        }

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Collections.Look(ref Props.metalNames, "metalNames", LookMode.Value);
        }

        public override void PostIngested(Pawn ingester) {
            Props.metalNames.ForEach(metal => {
                AllomancyUtility.AddMetalReserve(ingester, metal.ToLower(), 100f); // or adjust amount
            });
            Messages.Message($"{ingester.LabelShortCap} absorbed: {string.Join(", ", Props.metalNames)}.", ingester, MessageTypeDefOf.PositiveEvent);
        }

        public override string CompInspectStringExtra() {
            return $"Contains: {string.Join(", ", Props.metalNames.Select(metal => metal.CapitalizeFirst()))}";
        }
    }
}