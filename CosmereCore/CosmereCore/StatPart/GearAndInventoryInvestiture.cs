using CosmereCore.Extension;
using RimWorld;
using Verse;
using StatUtility = CosmereFramework.Util.StatUtility;

namespace CosmereCore.StatPart;

public class GearAndInventoryInvestiture : RimWorld.StatPart {
    public override void TransformValue(StatRequest req, ref float val) {
        if (TryGetValue(req, out float value)) {
            val += value;
        }
    }

    public override string? ExplanationPart(StatRequest req) {
        if (TryGetValue(req, out float value)) {
            return "CC_StatsReport_GearAndInventoryInvestiture".Translate() +
                   ": " +
                   value.ToStringBreathEquivalentUnitsRaw();
        }

        return null;
    }

    private bool TryGetValue(StatRequest req, out float value) {
        return StatUtility.TryGetPawnStat(
            req,
            parentStat,
            StatUtility.GearAndInventoryStat,
            StatUtility.StatBase,
            out value
        );
    }
}