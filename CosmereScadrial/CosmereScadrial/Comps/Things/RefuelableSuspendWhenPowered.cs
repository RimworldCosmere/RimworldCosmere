using RimWorld;
using Verse;

namespace CosmereScadrial.Comps.Things {
    public class RefuelableSuspendWhenPowered : CompRefuelable {
        public override void CompTick() {
            if (parent.TryGetComp<CompPowerTrader>()?.PowerOn == true) {
                // Pause fuel drain if powered
                return;
            }

            base.CompTick();
        }

        public override void CompTickRare() {
            if (parent.TryGetComp<CompPowerTrader>()?.PowerOn == true) {
                return;
            }

            base.CompTickRare();
        }

        public override void CompTickLong() {
            if (parent.TryGetComp<CompPowerTrader>()?.PowerOn == true) {
                return;
            }

            base.CompTickLong();
        }
    }
}