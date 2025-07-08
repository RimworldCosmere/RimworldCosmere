using CosmereCore.Extension;
using CosmereScadrial.Feruchemy.Comp.Thing;
using RimWorld;
using Verse;

namespace CosmereScadrial.StatPart;

public class MetalmindInvestiture : RimWorld.StatPart {
    public override void TransformValue(StatRequest req, ref float val) {
        if (TryGetValue(req, out float value)) {
            val += value;
        }
    }

    public override string ExplanationPart(StatRequest req) {
        if (TryGetValue(req, out float value)) {
            return "CS_StatsReport_MetalmindInvestiture".Translate() +
                   ": " +
                   value.ToStringBreathEquivalentUnitsRaw();
        }

        return null;
    }

    private bool TryGetValue(StatRequest req, out float value) {
        if (!req.Thing?.def?.thingCategories?.Contains(ThingCategoryDefOf.Cosmere_Scadrial_ThingCategory_Metalmind) ??
            true) {
            value = 0;
            return false;
        }

        Metalmind? comp = req.Thing.TryGetComp<Metalmind>();
        // Trying to balance out the investiture here
        value = (comp?.StoredAmount ?? 0) * Constants.BreathEquivalentUnitsPerMetalUnit / 3 / 16;

        return true;
    }
}