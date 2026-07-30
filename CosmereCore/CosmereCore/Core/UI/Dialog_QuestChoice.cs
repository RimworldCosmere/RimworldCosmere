using System.Collections.Generic;
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

    public override Vector2 InitialSize => new Vector2(WindowWidth, CalcHeight());

    public override void DoWindowContents(Rect inRect) {
        List<QuestChoiceOption>? options = part.options;
        if (options == null || options.Count == 0) {
            Close();
            return;
        }

        Rect body = inRect.ContractedBy(Spacing.Get());
        float y = body.y;

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

            Widgets.DrawBoxSolid(row, RowColor);
            Widgets.DrawHighlightIfMouseover(row);
            TooltipHandler.TipRegion(row, $"CC_Quest_Choice_{option.key}_Tip".Translate());
            MouseoverSounds.DoRegion(row);

            Rect inner = row.ContractedBy(Spacing.Get(0.5f));
            Rect labelRect = inner;
            if (option.silverCost > 0) labelRect.width -= Spacing.Get(5f);

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white)) {
                Widgets.Label(labelRect, $"CC_Quest_Choice_{option.key}".Translate());
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

    private static float RowHeight(QuestChoiceOption option, float rowWidth) {
        float labelWidth = rowWidth - Spacing.Get() - (option.silverCost > 0 ? Spacing.Get(5f) : 0f);
        float textHeight = Text.CalcHeight($"CC_Quest_Choice_{option.key}".Translate(), labelWidth);
        return Mathf.Max(textHeight + Spacing.Get(), Spacing.Get(3f));
    }
}
