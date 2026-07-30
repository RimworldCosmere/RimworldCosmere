using System.Collections.Generic;
using Cosmere.Core.Quest;
using Cosmere.Core.Quest.Objective;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI;

/// <summary>
///     Asks the player which branch of a quest stage to take. Modal: the quest cannot proceed
///     until the player answers, so the window cannot be dismissed without choosing one of the
///     options. Themed with a muted Scadrial steel-blue accent, matching the Ministry Convoy
///     and Crystal in the Deep quests this backs.
/// </summary>
public class Dialog_QuestChoice : Verse.Window {
    private const float WindowWidth = 480f;
    private const float AccentBarHeight = 3f;
    private const float DividerHeight = 1f;

    private static readonly Color AccentColor = new Color(0.42f, 0.58f, 0.66f);
    private static readonly Color RowColor = new Color(0.16f, 0.16f, 0.19f, 0.65f);
    private static readonly Color DividerColor = new Color(0.35f, 0.35f, 0.4f);
    private static readonly Color CostColor = new Color(0.82f, 0.8f, 0.68f);
    private static readonly Color BlurbColor = new Color(0.75f, 0.75f, 0.75f);

    // Blocked-row wash: the accent colour itself at a fraction of its alpha, not a new hue -
    // reads as "the same steel-blue surface, faded" rather than an unrelated warning colour.
    private static readonly Color BlockedRowColor = new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.15f);

    private readonly QuestPart_CosmereChoice part;

    public Dialog_QuestChoice(QuestPart_CosmereChoice part) {
        this.part = part;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        closeOnAccept = false;
        closeOnCancel = false;
        doCloseX = false;
        draggable = false;
    }

    // Window.InnerWindowOnGUI contracts the window rect by Margin (18f) before calling
    // DoWindowContents. Overriding to 0 leaves Spacing.Get() as the only contraction in play,
    // so CalcHeight's content width matches what DoWindowContents actually renders at.
    protected override float Margin => 0f;

    public override Vector2 InitialSize => new Vector2(WindowWidth, CalcHeight());

    public override void DoWindowContents(Rect inRect) {
        List<QuestChoiceOption>? options = part.options;
        if (options == null || options.Count == 0) {
            Close();
            return;
        }

        Rect body = inRect.ContractedBy(Spacing.Get());
        float y = body.y;
        int totalSilver = QuestSilver.GetTotalSilver();

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white)) {
            Rect title = new Rect(body.x, y, body.width, Text.CalcHeight("CC_Quest_Choice_Title".Translate(), body.width));
            Widgets.Label(title, "CC_Quest_Choice_Title".Translate());
            y = title.yMax + Spacing.Get(0.25f);
        }

        Widgets.DrawBoxSolid(new Rect(body.x, y, body.width, AccentBarHeight), AccentColor);
        y += AccentBarHeight + Spacing.Get();

        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, BlurbColor)) {
            Rect blurb = new Rect(body.x, y, body.width, Text.CalcHeight("CC_Quest_Choice_Blurb".Translate(), body.width));
            Widgets.Label(blurb, "CC_Quest_Choice_Blurb".Translate());
            y = blurb.yMax + Spacing.Get();
        }

        for (int i = 0; i < options.Count; i++) {
            QuestChoiceOption option = options[i];
            float rowHeight = RowHeight(option, body.width);
            Rect row = new Rect(body.x, y, body.width, rowHeight);
            bool blocked = option.silverCost > totalSilver;

            Widgets.DrawBoxSolid(row, blocked ? BlockedRowColor : RowColor);
            Widgets.DrawHighlightIfMouseover(row);
            TooltipHandler.TipRegion(row, BuildTip(option, blocked, totalSilver));
            MouseoverSounds.DoRegion(row);

            Rect inner = row.ContractedBy(Spacing.Get(0.5f));
            Rect labelRect = inner;
            if (option.silverCost > 0) labelRect.width -= Spacing.Get(5f);

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, blocked ? BlurbColor : Color.white)) {
                Widgets.Label(labelRect, ResolvedLabelKey(option).Translate());
            }

            if (option.silverCost > 0) {
                Rect costRect = new Rect(inner.xMax - Spacing.Get(5f), inner.y, Spacing.Get(5f), inner.height);
                using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleRight, CostColor)) {
                    Widgets.Label(costRect, ((float)option.silverCost).ToStringMoney());
                }
            }

            // ButtonInvisible's own doMouseoverSound is skipped - MouseoverSounds.DoRegion
            // above already covers the row every hovered frame, not just on the click frame.
            if (Widgets.ButtonInvisible(row, false) && part.Choose(option)) {
                Close();
                return;
            }

            y = row.yMax + Spacing.Get(0.5f);

            if (i < options.Count - 1) {
                Widgets.DrawBoxSolid(new Rect(body.x, y, body.width, DividerHeight), DividerColor);
                y += DividerHeight + Spacing.Get(0.5f);
            }
        }
    }

    private float CalcHeight() {
        List<QuestChoiceOption>? options = part.options;
        float contentWidth = WindowWidth - Spacing.Get() * 2;

        float height = Spacing.Get() * 2;
        height += Text.CalcHeight("CC_Quest_Choice_Title".Translate(), contentWidth);
        height += Spacing.Get(0.25f) + AccentBarHeight + Spacing.Get();
        height += Text.CalcHeight("CC_Quest_Choice_Blurb".Translate(), contentWidth);
        height += Spacing.Get();

        if (options == null) return height;

        for (int i = 0; i < options.Count; i++) {
            height += RowHeight(options[i], contentWidth) + Spacing.Get(0.5f);
            if (i < options.Count - 1) height += DividerHeight + Spacing.Get(0.5f);
        }

        return height;
    }

    private static string BuildTip(QuestChoiceOption option, bool blocked, int totalSilver) {
        string tip = ResolvedTipKey(option).Translate();
        if (!blocked) return tip;

        int shortfall = option.silverCost - totalSilver;
        string shortfallTip = "CC_Quest_Choice_InsufficientSilver_Tip"
            .Translate(((float)shortfall).ToStringMoney().Named("COST"))
            .Resolve();

        return shortfallTip + "\n\n" + tip;
    }

    private static float RowHeight(QuestChoiceOption option, float rowWidth) {
        float labelWidth = rowWidth - Spacing.Get() - (option.silverCost > 0 ? Spacing.Get(5f) : 0f);
        float textHeight = Text.CalcHeight(ResolvedLabelKey(option).Translate(), labelWidth);
        return Mathf.Max(textHeight + Spacing.Get(), Spacing.Get(3f));
    }

    // option.labelKey/tipKey are string? on the model (Scribe-compatible), but ChoiceObjective's
    // ConfigError rejects null/empty at load time, so a rendered dialog never sees the fallback.
    // Local-copy-then-narrow mirrors the idiom in Core/Quest/Prereq/FlagPrereq.cs.
    private static string ResolvedLabelKey(QuestChoiceOption option) {
        string? labelKey = option.labelKey;
        return labelKey != null && labelKey.Length > 0 ? labelKey : string.Empty;
    }

    private static string ResolvedTipKey(QuestChoiceOption option) {
        string? tipKey = option.tipKey;
        return tipKey != null && tipKey.Length > 0 ? tipKey : string.Empty;
    }
}
