using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using ScadrialSkillDefOf = Cosmere.System.Scadrial.SkillDefOf;
using RosharSkillDefOf = Cosmere.System.Roshar.SkillDefOf;

namespace Cosmere.Core.Patch;

[HarmonyPatch(typeof(SkillUI), nameof(SkillUI.DrawSkillsOf))]
public static class SkillVisibilityPatch {
    private static readonly FieldInfo LevelLabelWidthField =
        AccessTools.Field(typeof(SkillUI), "levelLabelWidth");

    private static readonly List<SkillDef> SkillDefsInListOrderCached =
        (List<SkillDef>)AccessTools.Field(typeof(SkillUI), "skillDefsInListOrderCached").GetValue(null);

    public static bool Prefix(Pawn p, Vector2 offset, SkillUI.SkillDrawMode mode) {
        Text.Font = GameFont.Small;
        if (p.DevelopmentalStage.Baby()) return true;

        List<SkillDef> allDefs = DefDatabase<SkillDef>.AllDefsListForReading;
        float levelLabelWidth = (float)LevelLabelWidthField.GetValue(null);
        for (int i = 0; i < allDefs.Count; i++) {
            float x = Text.CalcSize(allDefs[i].skillLabel.CapitalizeFirst()).x;
            if (x > levelLabelWidth) {
                levelLabelWidth = x;
            }
        }

        LevelLabelWidthField.SetValue(null, levelLabelWidth);

        int drawIndex = 0;
        for (int j = 0; j < SkillDefsInListOrderCached.Count; j++) {
            SkillDef skillDef = SkillDefsInListOrderCached[j];
            if (!ShouldShowSkill(p, skillDef)) continue;

            float y = drawIndex * 27f + offset.y;
            SkillUI.DrawSkill(p.skills.GetSkill(skillDef), new Vector2(offset.x, y), mode);
            drawIndex++;
        }

        return false;
    }

    public static bool ShouldShowSkill(Pawn pawn, SkillDef skillDef) {
        if (pawn.skills == null || pawn.genes == null || pawn.story == null) return true;

        if (skillDef == ScadrialSkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower) {
            return pawn.IsAllomancer() || pawn.IsMistborn();
        }

        if (skillDef == ScadrialSkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower) {
            return pawn.IsFeruchemist() || pawn.IsFullFeruchemist();
        }

        if (skillDef == RosharSkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower) {
            return pawn.IsSurgebinder();
        }

        return true;
    }

    public static int GetHiddenSkillCount(Pawn pawn) {
        int hidden = 0;
        for (int i = 0; i < SkillDefsInListOrderCached.Count; i++) {
            if (!ShouldShowSkill(pawn, SkillDefsInListOrderCached[i])) {
                hidden++;
            }
        }

        return hidden;
    }
}