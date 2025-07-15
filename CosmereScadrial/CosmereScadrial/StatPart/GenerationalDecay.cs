using CosmereScadrial.Extension;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.StatPart;

public class GenerationalDecay : RimWorld.StatPart {
    private const float MinimumValue = 0.1f;
    private const float GenerationalMultiplier = 0.5f;
    private bool allomancy => parentStat.Equals(StatDefOf.Cosmere_Scadrial_Stat_AllomanticPower);
    private bool feruchemy => parentStat.Equals(StatDefOf.Cosmere_Scadrial_Stat_FeruchemicPower);

    private StatDef stat => allomancy
        ? StatDefOf.Cosmere_Scadrial_Stat_AllomanticPower
        : StatDefOf.Cosmere_Scadrial_Stat_FeruchemicPower;

    public override void TransformValue(StatRequest req, ref float val) {
        if (!TryGetValue(req, out (float, float, float, float) value)) {
            return;
        }

        (float _, float _, float _, float final) = value;

        val += Mathf.Max(MinimumValue, final);
    }

    public override string? ExplanationPart(StatRequest req) {
        if (!TryGetValue(req, out (float, float, float, float) value)) {
            return null;
        }

        (float maternal, float paternal, float _, float final) = value;

        if (maternal > 0f || paternal > 0f) {
            return "CS_StatsReport_GenerationalDecay".Translate(
                $"{maternal:P}".Named("MATERNAL"),
                $"{paternal:P}".Named("PATERNAL"),
                $"{final:P}".Named("TOTAL")
            );
        }

        return "CS_StatsReport_GenerationalDecay_Unknown".Translate($"{MinimumValue:P}".Named("TOTAL"));
    }

    private bool IsCorrectMetalborn(Pawn pawn) {
        return allomancy && pawn.IsAllomancer() || feruchemy && pawn.IsFeruchemist();
    }

    private bool TryGetValue(StatRequest req, out (float, float, float, float) value) {
        value = default;
        if (!req.HasThing || req.Thing is not Pawn pawn || !IsCorrectMetalborn(pawn)) {
            return false;
        }

        float maternal = GetInheritedPower(pawn.GetMother()) * GenerationalMultiplier;
        float paternal = GetInheritedPower(pawn.GetFather()) * GenerationalMultiplier;
        float total = Mathf.Clamp01(maternal + paternal);
        float final = Mathf.Max(MinimumValue, total);

        value = (maternal, paternal, total, final);
        return true;
    }

    private float GetInheritedPower(Pawn? pawn, int depth = 0) {
        if (pawn == null || depth > 10) {
            return 0f;
        }

        if (IsCorrectMetalborn(pawn)) {
            return pawn.GetStatValue(stat);
        }

        float fromMother = GetInheritedPower(pawn.GetMother(), depth + 1);
        float fromFather = GetInheritedPower(pawn.GetFather(), depth + 1);

        return fromMother + fromFather;
    }
}