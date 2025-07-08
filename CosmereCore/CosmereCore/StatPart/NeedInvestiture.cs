using CosmereCore.Extension;
using CosmereCore.Need;
using CosmereCore.Util;
using RimWorld;
using Verse;

namespace CosmereCore.StatPart;

public class NeedInvestiture : RimWorld.StatPart {
    public override void TransformValue(StatRequest req, ref float val) {
        if (TryGetValue(req, out float value)) {
            val += value;
        }
    }

    public override string? ExplanationPart(StatRequest req) {
        if (TryGetValue(req, out float value)) {
            return "CS_StatsReport_NeedInvestiture".Translate() + ": " + value.ToStringBreathEquivalentUnits();
        }

        return null;
    }

    private bool TryGetValue(StatRequest req, out float value) {
        return InvestitureStatUtility.TryGetPawnInvestitureStat(
            req,
            x => x.needs.TryGetNeed<Investiture>().CurLevel,
            x => x.statBases.Find(x => x.stat.Equals(parentStat)).value,
            out value
        );
    }
}