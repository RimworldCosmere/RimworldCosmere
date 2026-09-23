using Cosmere.Core.Nightwatcher;
using Cosmere.Core.UI;
using Cosmere.System.Roshar.Comp.Hediff;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Nightwatcher;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.System.Roshar.Dialog;

public class Dialog_NightwatcherEncounter : Window {
    private const float Pad = 12f;
    private const float Gap = 8f;
    private const float TitleHeight = 36f;
    private const float TabHeight = 34f;
    private const float RowHeaderHeight = 28f;
    private const float PanelHeight = 92f;
    private const float FooterHeight = 40f;
    private const float ButtonHeight = 36f;
    private const float ConfirmWidth = 180f;
    private const float ScrollbarWidth = 20f;

    private static readonly Color NightwatcherGreen = new Color(0.35f, 0.82f, 0.55f);
    private static readonly Color CurseColor = new Color(0.72f, 0.28f, 0.92f);
    private static readonly Color EffectColor = new Color(0.9f, 0.75f, 0.4f);
    private static readonly Color BodyText = new Color(0.85f, 0.85f, 0.85f);
    private static readonly Color MutedText = new Color(0.75f, 0.75f, 0.75f);
    private static readonly Color DimText = new Color(0.55f, 0.55f, 0.55f);
    private static readonly Color TabSelectedText = new Color(0.88f, 0.94f, 0.9f);
    private static readonly Color TabUnselectedText = new Color(0.66f, 0.7f, 0.67f);
    private static readonly Color TabHoverFill = new Color(1f, 1f, 1f, 0.04f);
    private static readonly Color RuleColor = new Color(1f, 1f, 1f, 0.12f);
    private static readonly Color RowSelectedFill = new Color(1f, 1f, 1f, 0.05f);
    private static readonly Color RowSelectedEdge = new Color(0.35f, 0.82f, 0.55f, 0.85f);
    private static readonly Color PanelFill = new Color(1f, 1f, 1f, 0.03f);

    private static string GetTierLabel(int tier) => tier switch {
        1 => "CRO_NW_Tier1_Label".Translate(),
        2 => "CRO_NW_Tier2_Label".Translate(),
        _ => "CRO_NW_Tier3_Label".Translate(),
    };

    private static string GetTierDesc(int tier) => tier switch {
        1 => "CRO_NW_Tier1_Desc".Translate(),
        2 => "CRO_NW_Tier2_Desc".Translate(),
        _ => "CRO_NW_Tier3_Desc".Translate(),
    };

    private readonly Pawn pawn;
    private readonly List<BoonRowLayout> rowLayouts = [];
    private readonly Dictionary<int, List<NightwatcherBoonDef>> tierBuckets = [];
    private Vector2 boonScrollPos;
    private NightwatcherCurseDef? drawnCurse;

    private List<NightwatcherBoonDef>? filteredBoons;
    private Phase phase = Phase.Opening;
    private NightwatcherBoonDef? selectedBoon;
    private string? selectedChoiceKey;
    private int selectedTier = 1;

    public Dialog_NightwatcherEncounter(Pawn pawn) {
        this.pawn = pawn;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        closeOnAccept = false;
        closeOnCancel = false;
        doCloseX = false;
    }

    private List<NightwatcherBoonDef> FilteredBoons {
        get {
            if (filteredBoons != null) return filteredBoons;
            List<NightwatcherBoonDef> all = DefDatabase<NightwatcherBoonDef>.AllDefsListForReading;
            filteredBoons = [];
            for (int i = 0; i < all.Count; i++) {
                if (all[i].requiresMod != null && !ModsConfig.IsActive(all[i].requiresMod)) continue;
                filteredBoons.Add(all[i]);
            }

            return filteredBoons;
        }
    }

    public override Vector2 InitialSize => new Vector2(720f, 680f);

    public override void DoWindowContents(Rect inRect) {
        switch (phase) {
            case Phase.Opening:
                DrawOpening(inRect);
                break;
            case Phase.BoonSelection:
                DrawBoonSelection(inRect);
                break;
            case Phase.CurseReveal:
                DrawCurseReveal(inRect);
                break;
        }
    }

    private void DrawOpening(Rect inRect) {
        float y = inRect.y + 20f;

        using (new TextBlock(GameFont.Medium, TextAnchor.UpperLeft, NightwatcherGreen)) {
            Widgets.Label(
                new Rect(inRect.x, y, inRect.width, 40f),
                "CRO_NW_Opening_Title".Translate()
            );
        }

        y += 48f;

        string openingText = "CRO_NW_Opening_Text".Translate(pawn.Named("PAWN"));
        float textHeight = UIText.WrappedHeight(openingText, inRect.width - 20f, GameFont.Small);
        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, BodyText)) {
            Widgets.Label(new Rect(inRect.x + 10f, y, inRect.width - 20f, textHeight), openingText);
        }

        y += textHeight + 20f;

        float btnW = 220f;
        Rect btn = new Rect(inRect.x + (inRect.width - btnW) / 2f, y, btnW, ButtonHeight);
        MouseoverSounds.DoRegion(btn);
        if (Widgets.ButtonText(btn, "CRO_NW_Opening_Ask".Translate())) {
            phase = Phase.BoonSelection;
        }
    }

    private struct BoonRowLayout {
        public string descText;
        public float descHeight;
        public string effectsText;
        public float effectsHeight;
        public float rowHeight;
    }

    // Clamped so a def with a stray tier still lands in exactly one tab instead of vanishing.
    private static int TierOf(NightwatcherBoonDef boon) => Mathf.Clamp(boon.powerTier, 1, 3);

    private List<NightwatcherBoonDef> BoonsForTier(int tier) {
        if (tierBuckets.TryGetValue(tier, out List<NightwatcherBoonDef>? cached)) return cached;

        List<NightwatcherBoonDef> bucket = [];
        List<NightwatcherBoonDef> all = FilteredBoons;
        for (int i = 0; i < all.Count; i++) {
            if (TierOf(all[i]) == tier) bucket.Add(all[i]);
        }

        tierBuckets[tier] = bucket;

        return bucket;
    }

    private void DrawBoonSelection(Rect inRect) {
        Rect titleRect = new Rect(inRect.x, inRect.y + Pad, inRect.width, TitleHeight);
        using (new TextBlock(GameFont.Medium, TextAnchor.UpperLeft, NightwatcherGreen)) {
            Widgets.Label(titleRect, "CRO_NW_Boon_Title".Translate());
        }

        Rect tabRect = new Rect(inRect.x, titleRect.yMax + 4f, inRect.width, TabHeight);
        DrawTierTabs(tabRect);
        Widgets.DrawBoxSolid(new Rect(inRect.x, tabRect.yMax, inRect.width, 1f), RuleColor);

        string tierDesc = GetTierDesc(selectedTier);
        float tierDescHeight = UIText.WrappedHeight(tierDesc, inRect.width - Pad * 2f, GameFont.Tiny);
        Rect tierDescRect = new Rect(inRect.x + Pad, tabRect.yMax + 1f + Gap, inRect.width - Pad * 2f, tierDescHeight);
        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, DimText)) {
            Widgets.Label(tierDescRect, tierDesc);
        }

        Rect footerRect = new Rect(inRect.x, inRect.yMax - FooterHeight, inRect.width, FooterHeight);
        Rect panelRect = new Rect(inRect.x, footerRect.y - Gap - PanelHeight, inRect.width, PanelHeight);

        float listTop = tierDescRect.yMax + Gap;
        Rect listRect = new Rect(inRect.x, listTop, inRect.width, Mathf.Max(0f, panelRect.y - Gap - listTop));

        DrawBoonList(listRect);
        DrawSelectionPanel(panelRect);
        DrawConfirm(footerRect);
    }

    private void DrawTierTabs(Rect rect) {
        float tabWidth = rect.width / 3f;
        for (int tier = 1; tier <= 3; tier++) {
            Rect tab = new Rect(rect.x + (tier - 1) * tabWidth, rect.y, tabWidth, rect.height);
            bool isSelected = selectedTier == tier;
            if (!isSelected && Mouse.IsOver(tab)) Widgets.DrawBoxSolid(tab, TabHoverFill);

            string label = "CRO_NW_Tier_Tab".Translate(
                GetTierLabel(tier).Named("LABEL"),
                BoonsForTier(tier).Count.Named("COUNT")
            );
            UIText.EllipsisLabel(
                tab.ContractedBy(4f, 0f),
                label,
                GameFont.Small,
                TextAnchor.MiddleCenter,
                isSelected ? TabSelectedText : TabUnselectedText
            );

            if (isSelected) {
                Widgets.DrawBoxSolid(new Rect(tab.x + 6f, tab.yMax - 2f, tab.width - 12f, 2f), NightwatcherGreen);
            }

            TooltipHandler.TipRegion(tab, GetTierDesc(tier));
            MouseoverSounds.DoRegion(tab);
            if (!Widgets.ButtonInvisible(tab)) continue;

            selectedTier = tier;
            boonScrollPos = Vector2.zero;
        }
    }

    private void DrawBoonList(Rect listRect) {
        List<NightwatcherBoonDef> boons = BoonsForTier(selectedTier);
        float contentWidth = listRect.width - ScrollbarWidth;
        float textWidth = contentWidth - Pad * 2f;

        rowLayouts.Clear();
        float totalHeight = 0f;
        for (int i = 0; i < boons.Count; i++) {
            NightwatcherBoonDef boon = boons[i];
            BoonRowLayout layout = default;
            layout.descText = boon.description.Formatted(pawn.Named("PAWN"));
            layout.descHeight = UIText.WrappedHeight(layout.descText, textWidth, GameFont.Tiny);
            layout.effectsText = NightwatcherEffectText.Boon(boon, selectedBoon == boon ? selectedChoiceKey : null);
            layout.effectsHeight = layout.effectsText.Length > 0
                ? UIText.WrappedHeight(layout.effectsText, textWidth, GameFont.Tiny) + 4f
                : 0f;
            layout.rowHeight = RowHeaderHeight + layout.descHeight + layout.effectsHeight + Pad;
            totalHeight += layout.rowHeight;
            rowLayouts.Add(layout);
        }

        Rect viewRect = new Rect(0f, 0f, contentWidth, totalHeight);
        Widgets.BeginScrollView(listRect, ref boonScrollPos, viewRect);

        float rowY = 0f;
        for (int i = 0; i < boons.Count; i++) {
            NightwatcherBoonDef boon = boons[i];
            BoonRowLayout layout = rowLayouts[i];
            Rect rowRect = new Rect(0f, rowY, viewRect.width, layout.rowHeight);
            rowY += layout.rowHeight;

            if (rowRect.yMax < boonScrollPos.y || rowRect.y > boonScrollPos.y + listRect.height) continue;

            if (selectedBoon == boon) {
                Widgets.DrawBoxSolid(rowRect, RowSelectedFill);
                Widgets.DrawBoxSolid(new Rect(rowRect.x, rowRect.y, 2f, rowRect.height), RowSelectedEdge);
            } else {
                Widgets.DrawHighlightIfMouseover(rowRect);
            }

            using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, NightwatcherGreen)) {
                Widgets.Label(new Rect(Pad, rowRect.y + 4f, textWidth, 24f), boon.LabelCap);
            }

            using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, MutedText)) {
                Widgets.Label(
                    new Rect(Pad, rowRect.y + RowHeaderHeight, textWidth, layout.descHeight),
                    layout.descText
                );
            }

            if (layout.effectsText.Length > 0) {
                using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, EffectColor)) {
                    Widgets.Label(
                        new Rect(
                            Pad,
                            rowRect.y + RowHeaderHeight + layout.descHeight + 4f,
                            textWidth,
                            layout.effectsHeight
                        ),
                        layout.effectsText
                    );
                }
            }

            bool needsChoice = boon.Applicator is INightwatcherChoiceProvider;
            TooltipHandler.TipRegion(
                rowRect,
                needsChoice ? "CRO_NW_Boon_RowChoiceTip".Translate() : "CRO_NW_Boon_RowTip".Translate()
            );
            MouseoverSounds.DoRegion(rowRect);
            if (Widgets.ButtonInvisible(rowRect)) {
                if (needsChoice) {
                    ShowChoiceMenu(boon);
                } else {
                    selectedBoon = boon;
                    selectedChoiceKey = null;
                }
            }

            Widgets.DrawLineHorizontal(4f, rowRect.yMax - 1f, rowRect.width - 8f);
        }

        Widgets.EndScrollView();
    }

    private void DrawSelectionPanel(Rect rect) {
        Widgets.DrawBoxSolid(rect, PanelFill);
        Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, rect.width, 1f), RuleColor);

        Rect inner = rect.ContractedBy(Pad, Gap);

        if (selectedBoon == null) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, DimText)) {
                Widgets.Label(inner, "CRO_NW_Boon_NoSelection".Translate());
            }

            return;
        }

        Rect nameRect = new Rect(inner.x, inner.y, inner.width, 26f);
        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, NightwatcherGreen)) {
            Widgets.Label(nameRect, selectedBoon.LabelCap);
        }

        string detail = NightwatcherEffectText.Boon(selectedBoon, selectedChoiceKey);
        if (detail.Length == 0) detail = selectedBoon.description.Formatted(pawn.Named("PAWN"));

        Rect detailRect = new Rect(inner.x, nameRect.yMax + 2f, inner.width, inner.yMax - nameRect.yMax - 2f);
        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, EffectColor)) {
            Widgets.Label(detailRect, detail);
        }

        // the panel is fixed height, so a long effect line reads in full only from the tooltip
        TooltipHandler.TipRegion(rect, detail);
    }

    private void DrawConfirm(Rect footer) {
        bool needsChoice = selectedBoon?.Applicator is INightwatcherChoiceProvider;
        bool canConfirm = selectedBoon != null && (!needsChoice || selectedChoiceKey != null);

        Rect btn = new Rect(footer.x + (footer.width - ConfirmWidth) / 2f, footer.y, ConfirmWidth, ButtonHeight);
        if (!canConfirm) {
            TooltipHandler.TipRegion(
                btn,
                selectedBoon == null
                    ? "CRO_NW_Boon_ConfirmNeedsBoon".Translate()
                    : "CRO_NW_Boon_ConfirmNeedsChoice".Translate()
            );
        }

        MouseoverSounds.DoRegion(btn);
        if (!Widgets.ButtonText(btn, "CRO_NW_Boon_Confirm".Translate(), active: canConfirm)) return;

        NightwatcherApplicationContext? context = selectedChoiceKey != null
            ? new NightwatcherApplicationContext(selectedChoiceKey)
            : null;
        NightwatcherSystem.ApplyBoon(pawn, selectedBoon!, context);
        drawnCurse = NightwatcherSystem.DrawCurse(selectedBoon!);
        phase = Phase.CurseReveal;
    }

    private void DrawCurseReveal(Rect inRect) {
        float y = inRect.y + 20f;

        using (new TextBlock(GameFont.Medium, TextAnchor.UpperLeft, CurseColor)) {
            Widgets.Label(
                new Rect(inRect.x, y, inRect.width, 40f),
                "CRO_NW_Curse_Title".Translate()
            );
        }

        y += 48f;

        string revealText = "CRO_NW_Curse_Text".Translate(
            pawn.Named("PAWN"),
            selectedBoon!.Named("BOON")
        );
        float textHeight = UIText.WrappedHeight(revealText, inRect.width - 20f, GameFont.Small);
        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, BodyText)) {
            Widgets.Label(new Rect(inRect.x + 10f, y, inRect.width - 20f, textHeight), revealText);
        }

        y += textHeight + 16f;

        if (drawnCurse != null) {
            using (new TextBlock(GameFont.Medium, TextAnchor.UpperLeft, CurseColor)) {
                Widgets.Label(
                    new Rect(inRect.x + 10f, y, inRect.width - 20f, 32f),
                    drawnCurse.LabelCap
                );
            }

            y += 34f;

            string curseDesc = drawnCurse.description.Formatted(pawn.Named("PAWN"));
            float curseDescH = UIText.WrappedHeight(curseDesc, inRect.width - 40f, GameFont.Small);
            using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, MutedText)) {
                Widgets.Label(new Rect(inRect.x + 20f, y, inRect.width - 40f, curseDescH), curseDesc);
            }

            y += curseDescH + 6f;

            string curseEffects = NightwatcherEffectText.Curse(drawnCurse);
            if (curseEffects.Length > 0) {
                float effectsH = UIText.WrappedHeight(curseEffects, inRect.width - 40f, GameFont.Tiny);
                using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, EffectColor)) {
                    Widgets.Label(new Rect(inRect.x + 20f, y, inRect.width - 40f, effectsH), curseEffects);
                }

                y += effectsH + 8f;
            }

            y += 8f;
        }

        float btnW = 160f;
        Rect btn = new Rect(inRect.x + (inRect.width - btnW) / 2f, y, btnW, ButtonHeight);
        MouseoverSounds.DoRegion(btn);
        if (!Widgets.ButtonText(btn, "CRO_NW_Curse_Accept".Translate())) return;

        if (drawnCurse != null) {
            NightwatcherSystem.ApplyCurse(pawn, drawnCurse);
        }

        NightwatcherVisit? comp = pawn.TryGetComp<NightwatcherVisit>();
        comp?.MarkVisited();

        SendResultLetter();
        Close();
    }

    private void ShowChoiceMenu(NightwatcherBoonDef boon) {
        if (boon.Applicator is not INightwatcherChoiceProvider provider) return;
        List<FloatMenuOption> options = [];
        foreach (NightwatcherChoice choice in provider.GetChoices(boon)) {
            NightwatcherChoice captured = choice;
            options.Add(
                new FloatMenuOption(
                    captured.Label,
                    () => {
                        selectedBoon = boon;
                        selectedChoiceKey = captured.Key;
                    }
                )
            );
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }

    private static string GetHediffLabel(HediffDef hediff) {
        if (hediff.stages != null && hediff.stages.Count > 0 && hediff.stages[0].label != null) {
            return hediff.stages[0].label.CapitalizeFirst();
        }

        return hediff.LabelCap;
    }

    private void SendResultLetter() {
        string boonLabel = selectedBoon?.LabelCap ?? string.Empty;
        string curseLabel = drawnCurse?.LabelCap ?? string.Empty;

        Find.LetterStack.ReceiveLetter(
            "CRO_NW_Letter_Title".Translate(pawn.Named("PAWN")),
            "CRO_NW_Letter_Text".Translate(
                pawn.Named("PAWN"),
                boonLabel.Named("BOON"),
                curseLabel.Named("CURSE")
            ),
            RimWorld.LetterDefOf.PositiveEvent,
            pawn
        );
    }

    private enum Phase {
        Opening,
        BoonSelection,
        CurseReveal,
    }
}
