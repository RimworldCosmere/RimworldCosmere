using System.Collections.Generic;
using CosmereScadrial.CompProperties;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Comps {
    public class CompAllomanticVial : ThingComp {
        public CompProperties_AllomanticVial Props => (CompProperties_AllomanticVial)props;

        public List<string> MetalNames = new List<string>();

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Collections.Look(ref MetalNames, "metalNames", LookMode.Value);
        }

        public override void PostIngested(Pawn ingester) {
            base.PostIngested(ingester);

            foreach (var metal in MetalNames) {
                // Example hook to your investiture system
                AllomancyUtility.AddMetalReserve(ingester, metal, 1f); // or adjust amount
                Messages.Message($"{ingester.LabelShortCap} absorbed {metal}.", ingester, MessageTypeDefOf.PositiveEvent);
            }
            
            parent.Destroy(); // Destroys the vial after use
        }

        public override string CompInspectStringExtra() {
            return $"Contains: {string.Join(", ", MetalNames)}";
        }
    }
}