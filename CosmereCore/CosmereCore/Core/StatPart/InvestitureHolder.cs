using RimWorld;
using Verse;

namespace Cosmere.Core.StatPart;

public class InvestitureHolder : RimWorld.StatPart {
    public override void TransformValue(StatRequest req, ref float val) {
        if (TryGetValue(req, out float value)) {
            val += value;
        }
    }

    public override string? ExplanationPart(StatRequest req) {
        if (TryGetValue(req, out float value)) {
            return "CC_StatsReport_InvestitureHolder".Translate() +
                   ": " +
                   value.ToStringBreathEquivalentUnitsRaw();
        }

        return null;
    }

    private bool TryGetValue(StatRequest req, out float value) {
        if (!req.HasThing || !req.Thing.TryGetComp(out Comp.Thing.InvestitureHolder investitureHolder)) {
            value = 0;
            return false;
        }

        value = investitureHolder.currentInvestiture;
        return true;
    }
}