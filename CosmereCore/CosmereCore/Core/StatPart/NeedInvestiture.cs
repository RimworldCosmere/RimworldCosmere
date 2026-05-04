using RimWorld;
using Verse;
using StatUtility = Cosmere.Core.Util.StatUtility;

namespace Cosmere.Core.StatPart;

public class NeedInvestiture : RimWorld.StatPart {
    public override void TransformValue(StatRequest req, ref float val) {
        if (TryGetValue(req, out float value)) {
            val += value;
        }
    }

    public override string? ExplanationPart(StatRequest req) {
        if (TryGetValue(req, out float value)) {
            return "CC_StatsReport_NeedInvestiture".Translate() + ": " + value.ToStringBreathEquivalentUnits();
        }

        return null;
    }

    private bool TryGetValue(StatRequest req, out float value) {
        return StatUtility.TryGetPawnStat(
            req,
            parentStat,
            (_, p) => StatUtility.NeedLevel<Need.Investiture>(p),
            StatUtility.StatBase,
            out value
        );
    }
}