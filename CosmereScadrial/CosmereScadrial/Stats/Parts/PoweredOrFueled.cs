using System;
using RimWorld;
using Verse;

namespace CosmereScadrial.Stats.Parts {
    public class PoweredOrFueled : StatPart {
        private Tuple<bool, bool, bool> GetPowerAndFuel(StatRequest req) {
            if (req.Thing == null) return new Tuple<bool, bool, bool>(false, false, false);

            if (!req.Thing.def.HasModExtension<DefModExtensions.PoweredOrFueled>()) return new Tuple<bool, bool, bool>(false, false, false);

            var power = req.Thing.TryGetComp<CompPowerTrader>();
            var fuel = req.Thing.TryGetComp<CompRefuelable>();

            var hasPower = power?.PowerNet?.CurrentStoredEnergy() > 0 || power?.PowerNet?.CurrentEnergyGainRate() > 0;
            var hasFuel = fuel?.HasFuel ?? false;

            return new Tuple<bool, bool, bool>(true, hasPower, hasFuel);
        }

        public override void TransformValue(StatRequest req, ref float val) {
            var (success, hasPower, hasFuel) = GetPowerAndFuel(req);
            if (!success) return;

            if (hasPower) {
                val *= 2f;
            }
            else if (hasFuel) {
                val *= 1f;
            }
            else {
                val *= 0.2f;
            }
        }

        public override string ExplanationPart(StatRequest req) {
            var (success, hasPower, hasFuel) = GetPowerAndFuel(req);
            if (!success) return null;

            if (hasPower) {
                return "Powered: ×2.0";
            }

            return hasFuel ? "Fueled: ×1.0" : "Unfueled and unpowered: ×0.2";
        }
    }
}