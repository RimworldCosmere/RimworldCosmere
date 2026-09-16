using Cosmere.Core;
using Cosmere.Core.Ability.Autocast;
using Cosmere.Core.Savant;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Codex;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using Cosmere.System.Scadrial.Savant;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.System.Scadrial.UI;

[StaticConstructorOnStartup]
public sealed class AllomancyCodexContent : ICodexContentProvider {
    private static readonly Color RowStripeColor = new Color(1f, 1f, 1f, 0.03f);
    private static readonly Color OverallSkillColor = new Color(0.85f, 0.85f, 0.85f);
    private static readonly Color SecondaryTextColor = new Color(0.75f, 0.75f, 0.75f);
    private static readonly Color DimmedVialColor = new Color(1f, 1f, 1f, 0.22f);

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

        // Inside a row, not around the table: the rows themselves run to the frame.
        const float rowPad = 4f;
        float y = rect.y;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white)) {
            Widgets.Label(
                CodexChrome.ContentHeader(rect, y, 30f),
                "CC_Codex_Allomancy_ProgressionHeader".Translate()
            );
        }

        y += 34f;

        SkillRecord? skill = pawn.skills?.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower);
        if (skill != null) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, OverallSkillColor)) {
                Widgets.Label(
                    CodexChrome.ContentHeader(rect, y, 24f),
                    "CC_Codex_Allomancy_OverallSkill".Translate(skill.Level.Named("LEVEL"))
                );
            }

            y += 28f;
        }

        // scroll view: seventeen metals overflow the panel; markSize anchors the row, rowHeight grows to hold it
        const float markSize = 44f;
        const float rowHeight = 52f;
        const float rowGap = 2f;
        Rect listRect = new Rect(rect.x, y, rect.width, rect.yMax - y);
        Rect viewRect = new Rect(0f, 0f, listRect.width - 20f, genes.Count * (rowHeight + rowGap));
        Widgets.BeginScrollView(listRect, ref state.ProgressionScroll, viewRect);

        List<(Allomancer gene, int stage, int ticks)> rows = SortedRows(pawn, genes);

        y = 0f;
        for (int i = 0; i < rows.Count; i++) {
            (Allomancer gene, int stage, int burningTicks) = rows[i];
            MetallicArtsMetalDef metal = gene.metal;
            bool hasVialControls = !metal.IsOneOf(MetallicArtsMetalDefOf.Duralumin, MetallicArtsMetalDefOf.Nicrosil);

            Rect row = new Rect(0f, y, viewRect.width, rowHeight);
            if (i % 2 == 0) Widgets.DrawBoxSolid(row, RowStripeColor);
            Widgets.DrawHighlightIfMouseover(row);
            MouseoverSounds.DoRegion(row);

            // metals own mark in its own colour, not an anonymous chip that only the colour distinguished
            Rect swatch = new Rect(row.x + rowPad, row.y + (rowHeight - markSize) / 2f, markSize, markSize);
            Texture2D? mark = metal.allomancy?.invertedIcon;
            if (mark != null) {
                Color prevMark = GUI.color;
                GUI.color = metal.color;
                GUI.DrawTexture(swatch, mark);
                GUI.color = prevMark;
            } else {
                Widgets.DrawBoxSolid(swatch.ContractedBy(markSize / 4f), metal.color);
            }

            // the name column is a fixed width, so a long metal gets cut here and the row tooltip carries it whole
            UIText.EllipsisLabel(
                new Rect(swatch.xMax + 8f, row.y, 130f, rowHeight),
                metal.LabelCap,
                GameFont.Small,
                TextAnchor.MiddleLeft,
                Color.white
            );

            Rect stageRect = new Rect(swatch.xMax + 146f, row.y, 140f, rowHeight);
            SavantUI.DrawSavantStage(stageRect, stage, metal.color);

            // same reading as Feruchemy and the savant stage; never-burned is its own sentence, not substituted in
            string burnedLabel = burningTicks > 0
                ? "CC_Codex_Allomancy_MetalBurned".Translate(
                    burningTicks.ToStringTicksToPeriod(false, true, false).Named("DURATION")
                ).Resolve()
                : (string)"CC_Codex_Allomancy_NeverBurned".Translate();

            const float buttonSize = 28f;
            Rect vialButtonRect = new Rect(
                row.xMax - buttonSize - rowPad,
                row.y + (rowHeight - buttonSize) / 2f,
                buttonSize,
                buttonSize
            );

            // runs to the vial button, not a fixed width, so four-figure burn totals dont push into it
            Rect burnedRect = new Rect(
                stageRect.xMax + 8f,
                row.y,
                vialButtonRect.x - stageRect.xMax - 16f,
                rowHeight
            );
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, SecondaryTextColor)) {
                Widgets.Label(burnedRect, burnedLabel);
            }

            TooltipHandler.TipRegion(
                row,
                "CC_Codex_Allomancy_RowTooltip".Translate(
                    metal.LabelCap.Named("METAL"),
                    SavantUI.StageLabel(stage).Named("STAGE"),
                    burnedLabel.Named("BURNED")
                )
            );

            bool clicked = false;
            if (hasVialControls) {
                clicked = DrawVialSettingsButton(vialButtonRect, gene);
            } else {
                DrawVialSettingsUnavailable(vialButtonRect, metal);
            }

            // the whole row opens the dialog; the vial icon stays as the thing that says so
            if (hasVialControls && (clicked || Widgets.ButtonInvisible(row))) {
                Find.WindowStack.Add(new Dialog_AllomancyRestockSlider(gene));
            }

            y += rowHeight + rowGap;
        }

        Widgets.EndScrollView();
    }

    public void DrawBonds(Rect rect, Pawn pawn, CodexState state) { }

    public void DrawMemories(Rect rect, Pawn pawn, CodexState state) { }

    /// <summary>Worked metals first, so the table answers "which am I savant in" at a glance.</summary>
    private static List<(Allomancer gene, int stage, int ticks)> SortedRows(Pawn pawn, List<Allomancer> genes) {
        List<(Allomancer gene, int stage, int ticks)> rows = [];
        for (int i = 0; i < genes.Count; i++) {
            Allomancer gene = genes[i];
            MetallicArtsMetalDef metal = gene.metal;
            int stage = SavantUtility.CanBeSavant(metal)
                ? ScadrialSavantUtility.GetAllomanticSavantStage(pawn, metal)
                : 0;
            int ticks = pawn.records != null
                ? (int)pawn.records.GetValue(RecordDefOf.GetTimeSpentBurningForMetal(metal))
                : 0;

            rows.Add((gene, stage, ticks));
        }

        rows.Sort((a, b) => CodexRowOrder.Compare(a.stage, a.ticks, b.stage, b.ticks));

        return rows;
    }

    private static bool DrawVialSettingsButton(Rect rect, Allomancer gene) {
        string thresholdLabel = Allomancer.ThresholdDisplayLabel(gene);

        string tooltip = "CC_Codex_Allomancy_VialSettings_Tooltip".Translate(
            gene.RequestedVialStock.Named("COUNT"),
            thresholdLabel.Named("THRESHOLD")
        );
        TooltipHandler.TipRegion(rect, tooltip);

        // a text label clipped at the panel edge; the vial icon says it in the space available instead
        return VialIcon != null
            ? Widgets.ButtonImage(rect, VialIcon, true)
            : Widgets.ButtonText(rect, "CC_Codex_Allomancy_VialSettings_Button".Translate());
    }

    /// <summary>
    ///     An empty column read as an oversight, not as a metal with no reserve to keep. A dimmed
    ///     vial shows the setting exists but does not apply here.
    /// </summary>
    private static void DrawVialSettingsUnavailable(Rect rect, MetallicArtsMetalDef metal) {
        TooltipHandler.TipRegion(
            rect,
            "CC_Codex_Allomancy_NoVialSettings".Translate(metal.LabelCap.Named("METAL"))
        );

        if (VialIcon == null) return;

        Color prev = GUI.color;
        GUI.color = DimmedVialColor;
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

    public IReadOnlyList<AutocastTarget> AutocastTargets(Pawn pawn) {
        List<AutocastTarget> targets = [];
        List<RimWorld.Ability> all = pawn.abilities?.AllAbilitiesForReading ?? [];
        for (int i = 0; i < all.Count; i++) {
            if (!OwnsAbility(all[i])) continue;

            targets.Add(
                new AutocastTarget(
                    AutocastRuleKind.Ability,
                    all[i].def.defName,
                    all[i].def.LabelCap,
                    all[i].def.uiIcon
                )
            );
        }

        return targets;
    }
}
