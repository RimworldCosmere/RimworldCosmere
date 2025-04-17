using System.Linq;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Comps.Things {
    public class AllomanticVial : ThingComp {
        public new Properties.AllomanticVial props => (Properties.AllomanticVial)base.props;

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Collections.Look(ref props.metals, "metals", LookMode.Value);
        }

        public override void PostIngested(Pawn ingester) {
            props.metals.ForEach(metal => {
                AllomancyUtility.AddMetalReserve(ingester, metal, 100f); // or adjust amount
            });
            Messages.Message($"{ingester.LabelShortCap} downed a vial containing: {string.Join(", ", props.metals)}.", ingester, MessageTypeDefOf.PositiveEvent);
        }

        public override string CompInspectStringExtra() {
            return $"Contains: {string.Join(", ", props.metals.Select(metal => metal.label))}";
        }
    }
}