using CosmereCore.Extension;
using CosmereCore.Util;
using RimWorld;
using Verse;

namespace CosmereCore.StatPart;

public class GearAndInventoryInvestiture : RimWorld.StatPart {
    public override void TransformValue(StatRequest req, ref float val) {
        if (TryGetValue(req, out float value)) {
            val += value;
        }
    }

    public override string ExplanationPart(StatRequest req) {
        if (TryGetValue(req, out float value)) {
            return "CS_StatsReport_GearAndInventoryInvestiture".Translate() +
                   ": " +
                   value.ToStringBreathEquivalentUnitsRaw();
        }

        return null;
    }

    private bool TryGetValue(StatRequest req, out float value) {
        return InvestitureStatUtility.TryGetPawnInvestitureStat(
            req,
            x => InvestitureStatUtility.GearAndInventoryInvestiture(x),
            _ => 0f,
            out value
        );
    }
}