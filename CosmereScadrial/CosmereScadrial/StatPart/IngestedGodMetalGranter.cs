using CosmereScadrial.Extension;
using RimWorld;
using Verse;

namespace CosmereScadrial.StatPart;

public class IngestedGodMetalGranter : RimWorld.StatPart {
    public bool allomancy = false;
    public bool feruchemy = false;

    public override void TransformValue(StatRequest req, ref float val) {
        if (!req.HasThing || req.Thing is not Pawn pawn) {
            return;
        }

        if (allomancy) {
            if (!pawn.IsAllomancer()) {
                return;
            }

            if (pawn.records.GetAsInt(RecordDefOf.Cosmere_Scadrial_Record_IngestedLerasium) > 0) {
                val += 1;
            } else {
                val += pawn.records.GetAsInt(RecordDefOf.Cosmere_Scadrial_Record_IngestedLerasiumAlloy) / 16f;
            }
        } else if (feruchemy) {
            if (!pawn.IsFeruchemist()) {
                return;
            }

            if (pawn.records.GetAsInt(RecordDefOf.Cosmere_Scadrial_Record_IngestedLeratium) > 0) {
                val += 1;
            } else {
                val += pawn.records.GetAsInt(RecordDefOf.Cosmere_Scadrial_Record_IngestedLeratiumAlloy) / 16f;
            }
        }
    }

    public override string? ExplanationPart(StatRequest req) {
        if (!req.HasThing || req.Thing is not Pawn pawn) {
            return null;
        }

        if (allomancy) {
            if (pawn.records.GetAsInt(RecordDefOf.Cosmere_Scadrial_Record_IngestedLerasium) > 0) {
                return "CS_StatsReport_IngestedLerasium".Translate() + ": 100%";
            }

            float amount = pawn.records.GetAsInt(RecordDefOf.Cosmere_Scadrial_Record_IngestedLerasiumAlloy) / 16f;

            if (amount > 0) {
                return "CS_StatsReport_IngestedLerasiumAlloy".Translate() + $": {amount:P}";
            }
        }

        if (feruchemy) {
            if (pawn.records.GetAsInt(RecordDefOf.Cosmere_Scadrial_Record_IngestedLeratium) > 0) {
                return "CS_StatsReport_IngestedLeratium".Translate() + ": 100%";
            }

            float amount = pawn.records.GetAsInt(RecordDefOf.Cosmere_Scadrial_Record_IngestedLeratiumAlloy) / 16f;

            if (amount > 0) {
                return "CS_StatsReport_IngestedLeratiumAlloy".Translate() + $": {amount:P}";
            }
        }

        return null;
    }
}