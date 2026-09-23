using System.Reflection;
using Concord;
using Cosmere.Core.DefModExtension;
using Cosmere.Core.UI.Model;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Patch;

[Patch(typeof(SkillUI))]
public static class SkillVisibilityPatch {
    [InjectField("levelLabelWidth")]
    private static float levelLabelWidth;

    /// <summary>
    ///     Read by reflection, not [InjectField]: Concord only rewrites injected-field accesses
    ///     inside injection methods, and GetHiddenSkillCount is a plain helper called from outside one.
    /// </summary>
    private static readonly FieldInfo SkillDefsInListOrderCachedField = typeof(SkillUI).GetField(
        "skillDefsInListOrderCached",
        BindingFlags.Static | BindingFlags.NonPublic
    )!;

    private static List<SkillDef> SkillDefsInListOrderCached =>
        (List<SkillDef>?)SkillDefsInListOrderCachedField.GetValue(null) ?? [];

    [Inject(At.Head, nameof(SkillUI.DrawSkillsOf))]
    private static Control BeforeDrawSkillsOf(Pawn p, Vector2 offset, SkillUI.SkillDrawMode mode) {
        Text.Font = GameFont.Small;
        if (p.DevelopmentalStage.Baby()) return Control.Continue;

        List<SkillDef> allDefs = DefDatabase<SkillDef>.AllDefsListForReading;
        for (int i = 0; i < allDefs.Count; i++) {
            float x = Text.CalcSize(allDefs[i].skillLabel.CapitalizeFirst()).x;
            if (x > levelLabelWidth) {
                levelLabelWidth = x;
            }
        }

        int drawIndex = 0;
        for (int j = 0; j < SkillDefsInListOrderCached.Count; j++) {
            SkillDef skillDef = SkillDefsInListOrderCached[j];
            if (!ShouldShowSkill(p, skillDef)) continue;

            float y = drawIndex * 27f + offset.y;
            SkillUI.DrawSkill(p.skills.GetSkill(skillDef), new Vector2(offset.x, y), mode);
            drawIndex++;
        }

        return Control.Cancel;
    }

    public static bool ShouldShowSkill(Pawn pawn, SkillDef skillDef) {
        if (pawn.skills == null || pawn.genes == null || pawn.story == null) return true;

        InvestitureSkillExtension? ext = skillDef.GetModExtension<InvestitureSkillExtension>();
        if (ext == null) return true;

        // a skill can be gated on carrying a gene, e.g. shapeshifting belongs to kandra only.
        if (!string.IsNullOrEmpty(ext.requiresGene)) {
            GeneDef? required = DefDatabase<GeneDef>.GetNamedSilentFail(ext.requiresGene);
            if (required == null || !pawn.genes.HasActiveGene(required)) return false;
        }

        if (string.IsNullOrEmpty(ext.requiresInvestitureSystem)) return true;

        return InvestitureProviderRegistry.IsInvestedIn(pawn, ext.requiresInvestitureSystem);
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
