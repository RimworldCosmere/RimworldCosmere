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

[StaticConstructorOnStartup]
public sealed class AllomancyCodexContent : ICodexContentProvider {
    public bool HasProgression(Pawn pawn) {
        return CollectAllomancers(pawn).Count > 0;
    }

    public bool ShowsMemoriesSubtab => false;

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

        // The body runs to the frame now, so content sets its own inset.
        const float contentPad = 4f;
        float y = rect.y;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white)) {
            Widgets.Label(
                new Rect(rect.x + contentPad, y, rect.width - contentPad, 30f),
                "CC_Codex_Allomancy_ProgressionHeader".Translate()
            );
        }
        y += 34f;

        SkillRecord? skill = pawn.skills?.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower);
        if (skill != null) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.85f, 0.85f, 0.85f))) {
                Widgets.Label(
                    new Rect(rect.x + contentPad, y, rect.width - contentPad, 24f),
                    "CC_Codex_Allomancy_OverallSkill".Translate(skill.Level.Named("LEVEL"))
                );
            }

            y += 28f;
        }

        // Seventeen metals never fitted the panel, and without a scroll view the
        // ones past the fold were simply cut off rather than reachable.
        // The mark is the row's anchor, so it gets the room to be recognised and
        // the row grows to hold it.
        const float markSize = 44f;
        const float rowHeight = 52f;
        const float rowGap = 2f;
        Rect listRect = new Rect(rect.x, y, rect.width, rect.yMax - y);
        Rect viewRect = new Rect(0f, 0f, listRect.width - 20f, genes.Count * (rowHeight + rowGap));
        Widgets.BeginScrollView(listRect, ref progressionScroll, viewRect);

        y = 0f;
        for (int i = 0; i < genes.Count; i++) {
            Allomancer gene = genes[i];
            MetallicArtsMetalDef metal = gene.metal;
            bool hasVialControls = !metal.IsOneOf(MetalDefOf.Duralumin, MetalDefOf.Nicrosil);

            Rect row = new Rect(0f, y, viewRect.width, rowHeight);
            if (i % 2 == 0) Widgets.DrawBoxSolid(row, new Color(1f, 1f, 1f, 0.03f));

            // The metal's own mark in the metal's own colour, rather than an
            // anonymous chip that only the colour distinguished.
            Rect swatch = new Rect(row.x + contentPad, row.y + (rowHeight - markSize) / 2f, markSize, markSize);
            Texture2D? mark = metal.allomancy?.invertedIcon;
            if (mark != null) {
                Color prevMark = GUI.color;
                GUI.color = metal.color;
                GUI.DrawTexture(swatch, mark);
                GUI.color = prevMark;
            }
            else {
                Widgets.DrawBoxSolid(swatch.ContractedBy(markSize / 4f), metal.color);
            }

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

            const float buttonSize = 28f;
            Rect vialButtonRect = new Rect(
                row.xMax - buttonSize - contentPad,
                row.y + (rowHeight - buttonSize) / 2f,
                buttonSize,
                buttonSize
            );

            if (hasVialControls) {
                DrawVialSettingsButton(vialButtonRect, gene);
            }
            else {
                DrawVialSettingsUnavailable(vialButtonRect, metal);
            }

            y += rowHeight + rowGap;
        }

        Widgets.EndScrollView();
    }

    private Vector2 progressionScroll;

    public void DrawBonds(Rect rect, Pawn pawn, CodexState state) { }
    public void DrawMemories(Rect rect, Pawn pawn, CodexState state) { }

    private static void DrawVialSettingsButton(Rect rect, Allomancer gene) {
        string thresholdLabel = Allomancer.ThresholdDisplayLabel(gene);

        string tooltip = "CC_Codex_Allomancy_VialSettings_Tooltip".Translate(
            gene.RequestedVialStock.Named("COUNT"),
            thresholdLabel.Named("THRESHOLD")
        );
        TooltipHandler.TipRegion(rect, tooltip);

        // A word never fit this button and was clipped by the panel edge. The vial
        // the setting is about says it in the space available.
        bool clicked = VialIcon != null
            ? Widgets.ButtonImage(rect, VialIcon, true)
            : Widgets.ButtonText(rect, "CC_Codex_Allomancy_VialSettings_Button".Translate());

        if (clicked) {
            Find.WindowStack.Add(new Dialog_AllomancyRestockSlider(gene));
        }
    }

    /// Nothing at all in this column read as an oversight rather than as a metal
    /// that has no reserve to keep. A dimmed vial says the setting exists and does
    /// not apply here.
    private static void DrawVialSettingsUnavailable(Rect rect, MetallicArtsMetalDef metal) {
        TooltipHandler.TipRegion(
            rect,
            "CC_Codex_Allomancy_NoVialSettings".Translate(metal.LabelCap.Named("METAL"))
        );

        if (VialIcon == null) return;

        Color prev = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.22f);
        GUI.DrawTexture(rect, VialIcon);
        GUI.color = prev;
    }

    private static Texture2D? cachedVialIcon;
    private static bool vialIconResolved;

    private static Texture2D? VialIcon {
        get {
            if (vialIconResolved) return cachedVialIcon;

            vialIconResolved = true;
            cachedVialIcon = DefDatabase<ThingDef>
                .GetNamedSilentFail("Cosmere_Scadrial_Thing_AllomanticVial")?.uiIcon;

            return cachedVialIcon;
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