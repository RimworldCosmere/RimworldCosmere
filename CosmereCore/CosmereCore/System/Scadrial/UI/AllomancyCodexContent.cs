using Cosmere.Core;
using Cosmere.Core.Savant;
using Cosmere.Core.UI.Codex;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Savant;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class AllomancyCodexContent : ICodexContentProvider {
    public bool HasProgression(Pawn pawn) {
        return CollectAllomancers(pawn).Count > 0;
    }

    public bool ShowsBondsSubtab => false;

    public bool HasBonds(Pawn pawn) {
        return false;
    }

    public bool HasMemories(Pawn pawn) {
        return false;
    }

    public bool OwnsAbility(Ability ability) {
        return ability is AllomancyAbility;
    }

    public string? HeaderLabelFor(Pawn pawn) {
        return null;
    }

    public void DrawProgression(Rect rect, Pawn pawn, CodexState state) {
        List<Allomancer> genes = CollectAllomancers(pawn);
        if (genes.Count == 0) return;

        float y = rect.y;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(new Rect(rect.x, y, rect.width, 30f), "CC_Codex_Allomancy_ProgressionHeader".Translate());
        y += 34f;

        SkillRecord? skill = pawn.skills?.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower);
        if (skill != null) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.85f, 0.85f, 0.85f))) {
                Widgets.Label(
                    new Rect(rect.x, y, rect.width, 24f),
                    "CC_Codex_Allomancy_OverallSkill".Translate(skill.Level.Named("LEVEL"))
                );
            }

            y += 28f;
        }

        for (int i = 0; i < genes.Count; i++) {
            Allomancer gene = genes[i];
            MetallicArtsMetalDef metal = gene.metal;
            bool hasVialControls = !metal.IsOneOf(MetalDefOf.Duralumin, MetalDefOf.Nicrosil);

            const float rowHeight = 40f;
            Rect row = new Rect(rect.x, y, rect.width, rowHeight);
            if (i % 2 == 0) Widgets.DrawBoxSolid(row, new Color(1f, 1f, 1f, 0.03f));

            Rect swatch = new Rect(row.x + 4f, row.y + 15f, 10f, 10f);
            Widgets.DrawBoxSolid(swatch, metal.color);

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white))
                Widgets.Label(new Rect(swatch.xMax + 8f, row.y, 130f, rowHeight), metal.LabelCap);

            int stage = SavantUtility.CanBeSavant(metal)
                ? ScadrialSavantUtility.GetAllomanticSavantStage(pawn, metal)
                : 0;
            Rect stageRect = new Rect(swatch.xMax + 146f, row.y, 140f, rowHeight);
            SavantUI.DrawSavantStage(stageRect, stage, metal.color);

            float burned = 0f;
            if (pawn.records != null) {
                RecordDef record = RecordDefOf.GetMetalBurnRecordForMetal(metal);
                burned = pawn.records.GetValue(record);
            }

            Rect burnedRect = new Rect(stageRect.xMax + 8f, row.y, 80f, rowHeight);
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.75f, 0.75f, 0.75f))) {
                Widgets.Label(
                    burnedRect,
                    "CC_Codex_Allomancy_MetalBurned".Translate(burned.ToString("F1").Named("AMOUNT"))
                );
            }

            if (hasVialControls) {
                const float buttonSize = 20f;
                Rect vialButtonRect = new Rect(
                    row.xMax - buttonSize - 4f,
                    row.y + (rowHeight - buttonSize) / 2f,
                    buttonSize,
                    buttonSize
                );
                DrawVialSettingsButton(vialButtonRect, gene);
            }

            y += rowHeight + 2f;
        }
    }

    public void DrawBonds(Rect rect, Pawn pawn, CodexState state) { }
    public void DrawMemories(Rect rect, Pawn pawn, CodexState state) { }

    private static void DrawVialSettingsButton(Rect rect, Allomancer gene) {
        string thresholdLabel = Allomancer.ThresholdDisplayLabel(gene);

        string tooltip = "CC_Codex_Allomancy_VialSettings_Tooltip".Translate(
            gene.RequestedVialStock.Named("COUNT"),
            thresholdLabel.Named("THRESHOLD")
        );
        TooltipHandler.TipRegion(rect, tooltip);

        if (Widgets.ButtonText(rect, "CC_Codex_Allomancy_VialSettings_Button".Translate())) {
            Find.WindowStack.Add(new Dialog_AllomancyRestockSlider(gene));
        }
    }

    private static List<Allomancer> CollectAllomancers(Pawn pawn) {
        List<Allomancer> result = [];
        if (pawn.genes == null) return result;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Allomancer a && !a.Overridden) result.Add(a);
        }

        return result;
    }
}