using System.Linq;
using CosmereCore.Needs;
using CosmereScadrial.Comps;
using CosmereScadrial.Registry;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Hediffs {
    public class HediffCompProperties_BurnMetal : HediffCompProperties {
        public string metal;
        public int metalUnitsPerHour = 10;

        public HediffCompProperties_BurnMetal() {
            compClass = typeof(HediffComp_BurnMetal);
        }
    }

    public class HediffComp_BurnMetal : HediffComp {
        private const int TICKS_PER_HOUR = 60000;
        private int lastBurnTick = -1;

        public HediffCompProperties_BurnMetal Props {
            get => (HediffCompProperties_BurnMetal)props;
        }

        public override void CompPostTick(ref float severityAdjustment) {
            base.CompPostTick(ref severityAdjustment);

            if (parent.pawn.IsHashIntervalTick(500)) {
                var comp = parent.pawn.GetComp<CompScadrialInvestiture>();
                if (comp == null) return;

                var investNeed = parent.pawn.needs?.TryGetNeed<Need_Investiture>();
                if (investNeed == null) return;

                // Initial burn or re-burn if it's been an hour
                if (lastBurnTick == -1 || Find.TickManager.TicksGame - lastBurnTick >= TICKS_PER_HOUR) {
                    TryBurn();
                }
            }

            if (parent.pawn.IsHashIntervalTick(30)) // every half second
            {
                TryEmitEffect();
            }
        }

        private void TryEmitEffect() {
            if (!parent.pawn.Spawned || parent.pawn.Map == null) {
                return;
            }

            var mote = MoteMaker.MakeStaticMote(
                parent.pawn.DrawPos,
                parent.pawn.Map,
                ThingDefOf.Mote_PsyfocusPulse, // ← swap this for your own graphic later
                1.5f); // scale

            MetalRegistry.Metals.TryGetValue(Props.metal, out var metal);

            mote.instanceColor = metal.Color;
        }

        private void TryBurn() {
            var beuRequired = AllomancyUtility.BEU_PER_METAL_UNIT * Props.metalUnitsPerHour;

            if (AllomancyUtility.TryBurnMetalForInvestiture(parent.pawn, Props.metal, beuRequired)) {
                lastBurnTick = Find.TickManager.TicksGame;
                parent.Severity = 1f;
            }
            else if (TryAutoRefuel(Props.metal, Props.metalUnitsPerHour)) {
                TryBurn(); // Retry after refuel
            }
            else {
                // Remove the hediff
                parent.pawn.health.RemoveHediff(parent);
            }
        }

        private bool TryAutoRefuel(string metal, float amount) {
            var vial = parent.pawn.inventory?.innerContainer?
                .FirstOrDefault(t => t.def.defName == $"Cosmere_Vial_{metal}");

            if (vial == null) return false;

            vial.Destroy();
            Log.Message($"{parent.pawn.NameShortColored} auto-refueled {metal} via vial.");
            var comp = parent.pawn.GetComp<CompScadrialInvestiture>();
            comp?.AddReserve(metal, 100f); // assume vial adds 100 units
            return true;
        }
    }
}