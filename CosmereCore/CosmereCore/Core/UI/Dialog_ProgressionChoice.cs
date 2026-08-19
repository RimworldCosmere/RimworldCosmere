using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI;

/// <summary>One branch of a progression fork: what it says, what it warns, what it does.</summary>
public class ProgressionChoiceOption(string label, string? tip, Action onChosen) {
    public readonly string Label = label;
    public readonly Action OnChosen = onChosen;
    public readonly string? Tip = tip;
}

/// <summary>
///     A story fork with hoverable rows instead of bare buttons. Vanilla's Dialog_MessageBox
///     carries no tooltip on either button, so a choice that permanently ends a campaign looked
///     exactly like one that did not. The rows here say what they will do before they are
///     clicked.
/// </summary>
public class Dialog_ProgressionChoice : Verse.Window {
    private const float WindowWidth = 520f;
    private const float AccentBarHeight = 5f;
    private const float DividerHeight = 1f;

    private static readonly Color AccentColor = new Color(0.42f, 0.58f, 0.66f);
    private static readonly Color RowColor = new Color(0.16f, 0.16f, 0.19f, 0.65f);
    private static readonly Color DividerColor = new Color(0.30f, 0.34f, 0.38f);
    private static readonly Color BlurbColor = new Color(0.78f, 0.78f, 0.75f);

    private readonly List<ProgressionChoiceOption> options;
    private readonly string text;
    private readonly string title;

    public Dialog_ProgressionChoice(string title, string text, List<ProgressionChoiceOption> options) {
        this.title = title;
        this.text = text;
        this.options = options;

        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        closeOnAccept = false;
        closeOnCancel = false;
        doCloseX = false;
        draggable = false;
    }

    /// <summary>
    ///     Margin to 0 leaves Spacing.Get() as the only contraction in play, so CalcHeight's
    ///     content width matches what DoWindowContents actually draws into.
    /// </summary>
    protected override float Margin => 0f;

    public override Vector2 InitialSize => new Vector2(WindowWidth, CalcHeight());

    public override void DoWindowContents(Rect inRect) {
        if (options.Count == 0) {
            Close();
            return;
        }

        Rect body = inRect.ContractedBy(Spacing.Get());
        float y = body.y;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white)) {
            Rect titleRect = new Rect(body.x, y, body.width, Text.CalcHeight(title, body.width));
            Widgets.Label(titleRect, title);
            y = titleRect.yMax + Spacing.Get(0.25f);
        }

        Widgets.DrawBoxSolid(new Rect(body.x, y, body.width, AccentBarHeight), AccentColor);
        y += AccentBarHeight + Spacing.Get();

        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, BlurbColor)) {
            Rect blurb = new Rect(body.x, y, body.width, Text.CalcHeight(text, body.width));
            Widgets.Label(blurb, text);
            y = blurb.yMax + Spacing.Get();
        }

        for (int i = 0; i < options.Count; i++) {
            ProgressionChoiceOption option = options[i];
            Rect row = new Rect(body.x, y, body.width, RowHeight(option.Label, body.width));

            Widgets.DrawBoxSolid(row, RowColor);
            Widgets.DrawHighlightIfMouseover(row);
            if (option.Tip != null && option.Tip.Length > 0) TooltipHandler.TipRegion(row, option.Tip);
            MouseoverSounds.DoRegion(row);

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white)) {
                Widgets.Label(row.ContractedBy(Spacing.Get(0.5f)), option.Label);
            }

            // MouseoverSounds already covers the row each hovered frame, so ButtonInvisible skips its own sound.
            if (Widgets.ButtonInvisible(row, false)) {
                Action chosen = option.OnChosen;
                Close();
                chosen();
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
        float contentWidth = WindowWidth - Spacing.Get() * 2;

        float height = Spacing.Get() * 2;
        height += Text.CalcHeight(title, contentWidth);
        height += Spacing.Get(0.25f) + AccentBarHeight + Spacing.Get();
        height += Text.CalcHeight(text, contentWidth);
        height += Spacing.Get();

        for (int i = 0; i < options.Count; i++) {
            height += RowHeight(options[i].Label, contentWidth) + Spacing.Get(0.5f);
            if (i < options.Count - 1) height += DividerHeight + Spacing.Get(0.5f);
        }

        return height;
    }

    private static float RowHeight(string label, float rowWidth) {
        float textHeight = Text.CalcHeight(label, rowWidth - Spacing.Get());
        return Mathf.Max(textHeight + Spacing.Get(), Spacing.Get(3f));
    }
}
