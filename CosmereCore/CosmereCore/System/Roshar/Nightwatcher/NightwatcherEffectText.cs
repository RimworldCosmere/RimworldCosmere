using Cosmere.Core.Nightwatcher;
using Cosmere.System.Roshar.Comp.Hediff;
using Cosmere.System.Roshar.Def;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Nightwatcher;

/// <summary>
///     One line of what a boon or curse actually does, shared by the picker dialog and the hediff
///     tooltip so a gift with no hediff of its own still says what it changed.
/// </summary>
public static class NightwatcherEffectText {
    public static string Boon(NightwatcherBoonDef boon, string? choiceKey = null) {
        List<string> effects = [];

        for (int i = 0; i < boon.skillBoosts.Count; i++) {
            BoonSkillBoost boost = boon.skillBoosts[i];
            effects.Add("CRO_NW_Effect_SkillBoost".Translate(
                boost.levels.Named("LEVELS"),
                boost.skill.LabelCap.Named("SKILL")
            ));
        }

        if (boon.grantTrait != null) {
            string label = boon.grantTrait.DataAtDegree(boon.grantTraitDegree).label.CapitalizeFirst();
            effects.Add("CRO_NW_Effect_Gains".Translate(label.Named("LABEL")));
        }

        if (boon.removeTrait != null) {
            string label = boon.removeTrait.degreeDatas[0].label.CapitalizeFirst();
            effects.Add("CRO_NW_Effect_Removes".Translate(label.Named("LABEL")));
        }

        if (boon.removeHediff != null) {
            effects.Add("CRO_NW_Effect_Cures".Translate(boon.removeHediff.LabelCap.Named("LABEL")));
        }

        if (boon.surgebindingConnectionBoost > 0f) {
            effects.Add("CRO_NW_Effect_CultivationConnection".Translate(
                boon.surgebindingConnectionBoost.ToString("0.##").Named("AMOUNT")
            ));
        }

        if (boon.psylinkBoost) {
            effects.Add("CRO_NW_Effect_PsylinkLevel".Translate());
        }

        if (boon.hediff != null) {
            AppendHediffEffects(boon.hediff, effects);
            if (boon.hediff.HasComp(typeof(AgelessBody))) {
                effects.Add("CRO_NW_Effect_BiologicalImmortality".Translate());
            }
        }

        if (boon.Applicator is INightwatcherEffectDescriber describer) {
            NightwatcherApplicationContext? ctx = choiceKey != null
                ? new NightwatcherApplicationContext(choiceKey)
                : null;
            string? extra = describer.DescribeEffects(ctx);
            if (!string.IsNullOrEmpty(extra)) effects.Add(extra!);
        }

        return effects.Count > 0 ? string.Join("  |  ", effects) : string.Empty;
    }

    public static string Curse(NightwatcherCurseDef curse) {
        List<string> effects = [];

        if (curse.forceTrait != null) {
            TraitDegreeData? degreeData = curse.forceTrait.degreeDatas
                                              .FirstOrDefault(d => d.degree == curse.forceTraitDegree) ??
                                          curse.forceTrait.degreeDatas.FirstOrDefault();
            if (degreeData != null) {
                effects.Add("CRO_NW_Effect_Gains".Translate(degreeData.label.CapitalizeFirst().Named("LABEL")));
            }
        }

        if (curse.stripTrait != null && curse.stripTrait.degreeDatas.Count > 0) {
            string label = curse.stripTrait.degreeDatas[0].label.CapitalizeFirst();
            effects.Add("CRO_NW_Effect_Loses".Translate(label.Named("LABEL")));
        }

        if (curse.penaltySkill != null) {
            effects.Add("CRO_NW_Effect_SkillPenalty".Translate(
                curse.penaltySkillLevels.Named("LEVELS"),
                curse.penaltySkill.LabelCap.Named("SKILL")
            ));
        }

        if (curse.hediff != null && !AppendHediffEffects(curse.hediff, effects)) {
            effects.Add(curse.hediff.LabelCap);
        }

        if (curse.Applicator is INightwatcherEffectDescriber describer) {
            string? extra = describer.DescribeEffects();
            if (!string.IsNullOrEmpty(extra)) effects.Add(extra!);
        }

        if (curse.cultivationEvolutionDays > 0) {
            float years = curse.cultivationEvolutionDays / 365f;
            effects.Add("CRO_NW_Effect_EvolvesAfterYears".Translate(years.ToString("0.#").Named("YEARS")));
        }

        return effects.Count > 0 ? string.Join("  |  ", effects) : string.Empty;
    }

    private static bool AppendHediffEffects(HediffDef hediff, List<string> effects) {
        if (hediff.stages == null || hediff.stages.Count == 0) return false;
        HediffStage stage = hediff.stages[0];
        int startCount = effects.Count;

        if (stage.statOffsets != null) {
            for (int i = 0; i < stage.statOffsets.Count; i++) {
                StatModifier mod = stage.statOffsets[i];
                string sign = mod.value >= 0 ? "+" : string.Empty;
                effects.Add("CRO_NW_Effect_StatOffset".Translate(
                    sign.Named("SIGN"),
                    mod.value.ToString("0.##").Named("VALUE"),
                    mod.stat.LabelCap.Named("STAT")
                ));
            }
        }

        if (stage.statFactors != null) {
            for (int i = 0; i < stage.statFactors.Count; i++) {
                StatModifier mod = stage.statFactors[i];
                float pct = (mod.value - 1f) * 100f;
                string sign = pct >= 0 ? "+" : string.Empty;
                effects.Add("CRO_NW_Effect_StatFactor".Translate(
                    sign.Named("SIGN"),
                    pct.ToString("0.#").Named("VALUE"),
                    mod.stat.LabelCap.Named("STAT")
                ));
            }
        }

        if (stage.capMods != null) {
            for (int i = 0; i < stage.capMods.Count; i++) {
                PawnCapacityModifier cap = stage.capMods[i];
                if (cap.offset != 0f) {
                    string sign = cap.offset >= 0 ? "+" : string.Empty;
                    effects.Add("CRO_NW_Effect_CapacityOffset".Translate(
                        sign.Named("SIGN"),
                        (cap.offset * 100f).ToString("0.#").Named("VALUE"),
                        cap.capacity.LabelCap.Named("CAPACITY")
                    ));
                }
            }
        }

        if (stage.painOffset != 0f) {
            string sign = stage.painOffset >= 0 ? "+" : string.Empty;
            effects.Add("CRO_NW_Effect_PainOffset".Translate(
                sign.Named("SIGN"),
                stage.painOffset.ToString("0.##").Named("VALUE")
            ));
        }

        return effects.Count > startCount;
    }
}
