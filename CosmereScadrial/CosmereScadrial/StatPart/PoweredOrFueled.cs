using System;
using RimWorld;
using Verse;

namespace CosmereScadrial.StatPart;

public class PoweredOrFueled : RimWorld.StatPart {
    private Tuple<bool, bool, bool> GetPowerAndFuel(StatRequest req) {
        if (req.Thing == null) return new Tuple<bool, bool, bool>(false, false, false);

        if (!req.Thing.def.HasModExtension<DefModExtension.PoweredOrFueled>()) {
            return new Tuple<bool, bool, bool>(false, false, false);
        }

        CompPowerTrader? power = req.Thing.TryGetComp<CompPowerTrader>();
        CompRefuelable? fuel = req.Thing.TryGetComp<CompRefuelable>();

        bool hasPower = power?.PowerNet?.CurrentStoredEnergy() > 0 || power?.PowerNet?.CurrentEnergyGainRate() > 0;
        bool hasFuel = fuel?.HasFuel ?? false;

        return new Tuple<bool, bool, bool>(true, hasPower, hasFuel);
    }

    public override void TransformValue(StatRequest req, ref float val) {
        (bool success, bool hasPower, bool hasFuel) = GetPowerAndFuel(req);
        if (!success) return;

        if (hasPower) {
            val *= 2f;
        } else if (hasFuel) {
            val *= 1f;
        } else {
            val *= 0.2f;
        }
    }

    public override string ExplanationPart(StatRequest req) {
        (bool success, bool hasPower, bool hasFuel) = GetPowerAndFuel(req);
        if (!success) return "";

        if (hasPower) {
            return "Powered: ×2.0";
        }

        return hasFuel ? "Fueled: ×1.0" : "Unfueled and unpowered: ×0.2";
    }
}