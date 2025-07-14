using RimWorld;
using Verse;
using StatUtility = CosmereFramework.Util.StatUtility;

namespace CosmereCore.StatPart;

public class BodyType : RimWorld.StatPart {
    private const int baseWeight = 60;

    public override void TransformValue(StatRequest req, ref float val) {
        if (TryGetBodyType(req, out float bodyType)) val *= bodyType;
    }

    public override string? ExplanationPart(StatRequest req) {
        if (TryGetBodyType(req, out float bodyType)) {
            Pawn? pawn = req.Pawn ?? req.Thing as Pawn;
            string bodyTypeDisplay = pawn?.story.bodyType?.defName ?? "N/A";
            return "CC_StatsReport_BodyType".Translate(bodyTypeDisplay) + ": x" + bodyType.ToStringPercent();
        }

        return null;
    }

    private bool TryGetBodyType(StatRequest req, out float bodyType) {
        return StatUtility.TryGetPawnStat(req, GetBodyTypeMultiplier, _ => 0f, out bodyType);
    }

    private float GetBodyTypeMultiplier(Pawn pawn) {
        float estimatedWeightKg = baseWeight; // default base

        BodyTypeDef? bodyType = pawn.story?.bodyType;

        if (bodyType == BodyTypeDefOf.Thin) {
            estimatedWeightKg = 55f;
        } else if (bodyType == BodyTypeDefOf.Fat) {
            estimatedWeightKg = 110f;
        } else if (bodyType == BodyTypeDefOf.Hulk) {
            estimatedWeightKg = 100f;
        } else if (bodyType == BodyTypeDefOf.Male) {
            estimatedWeightKg = 80f;
        } else if (bodyType == BodyTypeDefOf.Female) {
            estimatedWeightKg = 65f;
        }

        return estimatedWeightKg / baseWeight;
    }
}