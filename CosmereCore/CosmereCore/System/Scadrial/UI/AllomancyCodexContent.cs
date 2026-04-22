using System.Collections.Generic;
using Cosmere.Core.Savant;
using Cosmere.Core.UI.Codex;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class AllomancyCodexContent : ICodexContentProvider {
    public bool HasProgression(Pawn pawn) => CollectAllomancers(pawn).Count > 0;
    public bool ShowsBondsSubtab => false;
    public bool HasBonds(Pawn pawn) => false;
    public bool HasMemories(Pawn pawn) => false;
    public bool OwnsAbility(RimWorld.Ability ability) => ability is AllomancyAbility;

    public string? HeaderLabelFor(Pawn pawn) => null;

    public void DrawProgression(Pawn pawn, Rect rect) {
        List<Allomancer> genes = CollectAllomancers(pawn);
        if (genes.Count == 0) return;

        float y = rect.y;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(new Rect(rect.x, y, rect.width, 30f), "CC_Codex_Allomancy_ProgressionHeader".Translate());
        y += 34f;

        SkillRecord? skill = pawn.skills?.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower);
        if (skill != null) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.85f, 0.85f, 0.85f)))
                Widgets.Label(
                    new Rect(rect.x, y, rect.width, 24f),
                    "CC_Codex_Allomancy_OverallSkill".Translate(skill.Level.Named("LEVEL"))
                );
            y += 28f;
        }

        for (int i = 0; i < genes.Count; i++) {
            Allomancer gene = genes[i];
            MetallicArtsMetalDef metal = gene.metal;

            Rect row = new Rect(rect.x, y, rect.width, 26f);
            if (i % 2 == 0) Widgets.DrawBoxSolid(row, new Color(1f, 1f, 1f, 0.03f));

            Rect swatch = new Rect(row.x + 4f, row.y + 8f, 10f, 10f);
            Widgets.DrawBoxSolid(swatch, metal.color);

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white))
                Widgets.Label(new Rect(swatch.xMax + 8f, row.y, 130f, row.height), metal.LabelCap);

            int stage = SavantUtility.CanBeSavant(metal)
                ? SavantUtility.GetAllomanticSavantStage(pawn, metal)
                : 0;
            Rect stageRect = new Rect(swatch.xMax + 146f, row.y, 140f, row.height);
            DrawSavantStage(stageRect, stage, metal.color);

            float burned = 0f;
            if (pawn.records != null) {
                RecordDef record = RecordDefOf.GetMetalBurnRecordForMetal(metal);
                burned = pawn.records.GetValue(record);
            }

            Rect burnedRect = new Rect(stageRect.xMax + 8f, row.y, row.xMax - stageRect.xMax - 12f, row.height);
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.75f, 0.75f, 0.75f)))
                Widgets.Label(
                    burnedRect,
                    "CC_Codex_Allomancy_MetalBurned".Translate(burned.ToString("F1").Named("AMOUNT"))
                );

            y += 28f;
        }
    }

    public void DrawBonds(Pawn pawn, Rect rect) { }
    public void DrawMemories(Pawn pawn, Rect rect) { }

    private static void DrawSavantStage(Rect rect, int stage, Color accent) {
        string label;
        Color color;
        switch (stage) {
            case 1:
                label = (string)"CC_Codex_Savant_Stage1".Translate();
                color = Color.Lerp(accent, Color.white, 0.35f);
                break;
            case 2:
                label = (string)"CC_Codex_Savant_Stage2".Translate();
                color = accent;
                break;
            case 3:
                label = (string)"CC_Codex_Savant_Stage3".Translate();
                color = Color.Lerp(accent, new Color(1f, 0.9f, 0.4f), 0.5f);
                break;
            default:
                label = (string)"CC_Codex_Savant_Stage0".Translate();
                color = new Color(0.55f, 0.55f, 0.55f);
                break;
        }
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, color))
            Widgets.Label(rect, label);
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
