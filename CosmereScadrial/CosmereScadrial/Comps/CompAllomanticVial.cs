using System.Linq;
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
            Props.metalNames.ForEach(metal => {
                // Todo, add Investiture
                Log.Message($"{ingester.LabelShortCap} ingesting: {metal}");
                AllomancyUtility.AddMetalReserve(ingester, metal.ToLower(), 100f); // or adjust amount
            });
            Messages.Message($"{ingester.LabelShortCap} absorbed: {string.Join(", ", Props.metalNames)}.", ingester, MessageTypeDefOf.PositiveEvent);
        }

        public override string CompInspectStringExtra() {
            return $"Contains: {string.Join(", ", Props.metalNames.Select(metal => metal.CapitalizeFirst()))}";
        }
    }
}