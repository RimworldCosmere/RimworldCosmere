using System;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Dock;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Radial;

public static class RadialCenterPreview {
    // The header hangs from the top of the hub and the bar and buttons are
    // pinned to the bottom, so the description keeps whatever is left between
    // them rather than every row flowing from a single stack.
    private const float BreadcrumbY = -116f;
    private const float TitleGap = 22f;
    private const float ButtonRowY = 80f;
    private const float ButtonSize = 30f;
    private const float BarGap = 12f;
    private const float DescriptionGap = 10f;
    private const float TitleHintGap = 4f;
    private const float BarWidth = 201f;

    public static void Draw(Vector2 center, RadialLeaf? hoveredLeaf, string? hoveredTitle, string breadcrumb, bool browseMode, Action back, Action close) {
        float r = RadialLayout.CenterRadius;
        Rect disc = new Rect(center.x - r, center.y - r, r * 2f, r * 2f);
        Texture2D discTex = RadialWedgeTex.Disc();
        Color prev = GUI.color;
        GUI.color = DockPalette.Border;
        GUI.DrawTexture(disc.ExpandedBy(1.5f), discTex);
        GUI.color = new Color(0.078f, 0.098f, 0.125f, 0.96f);
        GUI.DrawTexture(disc, discTex);
        GUI.color = prev;

        float tinyH = Text.LineHeightOf(GameFont.Tiny);
        float smallH = Text.LineHeightOf(GameFont.Small);
        float mediumH = Text.LineHeightOf(GameFont.Medium);
        int titleLines = 1;
        bool canFlare = hoveredLeaf != null
            && hoveredLeaf.Kind == RadialActionKind.StartAllomancyBurn
            && !hoveredLeaf.IsLocked
            && hoveredLeaf.CanFlare;

        UIText.EllipsisLabel(ChordRow(center, BreadcrumbY, tinyH), breadcrumb, GameFont.Tiny, TextAnchor.MiddleCenter, DockPalette.GroupLabel);

        if (hoveredLeaf == null) {
            if (hoveredTitle != null) {
                UIText.EllipsisLabel(
                    ChordRow(center, -28f, smallH),
                    hoveredTitle,
                    GameFont.Small,
                    TextAnchor.MiddleCenter,
                    new Color(0.75f, 0.89f, 0.95f)
                );
                UIText.EllipsisLabel(
                    ChordRow(center, 0f, tinyH),
                    "CC_Radial_Open_Prompt".Translate(),
                    GameFont.Tiny,
                    TextAnchor.MiddleCenter,
                    DockPalette.MutedText
                );
            } else {
                UIText.EllipsisLabel(
                    ChordRow(center, -9f, tinyH),
                    "CC_Radial_Hover_Prompt".Translate(),
                    GameFont.Tiny,
                    TextAnchor.MiddleCenter,
                    DockPalette.MutedText
                );
            }
        } else {
            bool flareArmed = ShiftHeld() && hoveredLeaf.Kind == RadialActionKind.StartAllomancyBurn && !hoveredLeaf.IsLocked && hoveredLeaf.CanFlare;
            string title = flareArmed
                ? "CC_Radial_Action_Flare".Translate((hoveredTitle ?? hoveredLeaf.Label).Named("METAL"))
                : hoveredLeaf.Label;
            titleLines = DrawWrappedTitle(
                center,
                BreadcrumbY + TitleGap,
                mediumH,
                title,
                flareArmed ? DockPalette.Flare : new Color(0.75f, 0.89f, 0.95f)
            );

            float bodyTop = BreadcrumbY + TitleGap + mediumH * titleLines + (canFlare ? TitleHintGap + tinyH : 0f) + DescriptionGap;
            float bodyBottom = ButtonRowY - BarGap - (tinyH + 6f) - DescriptionGap;

            if (!hoveredLeaf.Description.NullOrEmpty()) {
                float extraRow = hoveredLeaf.IsLocked || BuildMetaLine(hoveredLeaf).Length > 0 ? tinyH : 0f;
                float descHeight = Mathf.Max(tinyH, bodyBottom - extraRow - bodyTop);
                Rect descRect = ChordRow(center, bodyTop, descHeight);
                int maxLines = Mathf.Max(1, Mathf.FloorToInt(descHeight / tinyH));
                using (new TextBlock(GameFont.Tiny, TextAnchor.UpperCenter, DockPalette.MutedText)) {
                    Widgets.Label(descRect, FitToLines(hoveredLeaf.Description!, descRect.width, maxLines));
                }

                TooltipHandler.TipRegion(descRect, hoveredLeaf.Description);
            }

            if (hoveredLeaf.IsLocked && hoveredLeaf.LockReason != null) {
                UIText.EllipsisLabel(ChordRow(center, bodyBottom - tinyH, tinyH), hoveredLeaf.LockReason, GameFont.Tiny, TextAnchor.MiddleCenter, DockPalette.Flare);
            } else {
                string meta = BuildMetaLine(hoveredLeaf);
                if (meta.Length > 0) {
                    UIText.EllipsisLabel(ChordRow(center, bodyBottom - tinyH, tinyH), meta, GameFont.Tiny, TextAnchor.MiddleCenter, DockPalette.HotLabel);
                }
            }

            if (hoveredLeaf.ReserveFraction.HasValue) {
                float fraction = Mathf.Clamp01(hoveredLeaf.ReserveFraction.Value);
                float barHeight = tinyH + 6f;

                // Pinned width: deriving it from the chord at the bar's own y
                // would resize the bar whenever the footer moves.
                Rect barRow = new Rect(
                    center.x - BarWidth / 2f,
                    center.y + ButtonRowY - BarGap - barHeight,
                    BarWidth,
                    barHeight
                );

                Widgets.DrawBoxSolid(barRow, new Color(0f, 0f, 0f, 0.55f));
                Widgets.DrawBoxSolid(
                    new Rect(barRow.x + 1f, barRow.y + 1f, (barRow.width - 2f) * fraction, barHeight - 2f),
                    new Color(0.24f, 0.42f, 0.50f)
                );
                Widgets.DrawBox(barRow, 1, BaseContent.WhiteTex);

                Rect barInner = barRow.ContractedBy(6f, 0f);
                UIText.EllipsisLabel(
                    barInner,
                    "CC_Radial_Reserve".Translate(Mathf.RoundToInt(fraction * 100f).Named("PERCENT")),
                    GameFont.Tiny,
                    TextAnchor.MiddleLeft,
                    new Color(0.92f, 0.95f, 0.97f)
                );

                string rate = BuildRateLine(hoveredLeaf);
                if (rate.Length > 0) {
                    UIText.EllipsisLabel(barInner, rate, GameFont.Tiny, TextAnchor.MiddleRight, DockPalette.HotLabel);
                }
            }
        }

        if (canFlare) {
            // The wheel stays open on a tap but casts on release when held, so
            // the verb has to match however this pawn's wheel was opened.
            string verb = browseMode ? "CC_Radial_Verb_Click".Translate() : "CC_Radial_Verb_Release".Translate();
            bool shiftHeld = ShiftHeld();
            UIText.EllipsisLabel(
                ChordRow(center, BreadcrumbY + TitleGap + mediumH * titleLines + TitleHintGap, tinyH),
                shiftHeld
                    ? "CC_Radial_Hint_FlareOnly".Translate(verb.Named("VERB"))
                    : "CC_Radial_Hint_BurnFlare".Translate(verb.Named("VERB")),
                GameFont.Tiny,
                TextAnchor.MiddleCenter,
                shiftHeld ? DockPalette.Flare : DockPalette.GroupLabel
            );
        }

        if (browseMode) {
            const float buttonSize = ButtonSize;
            const float buttonGap = 8f;
            float buttonY = center.y + ButtonRowY;
            RimWorld.AbilityDef? infoDef = hoveredLeaf?.AbilityDef;
            int buttonCount = infoDef != null ? 3 : 2;
            float rowWidth = buttonCount * buttonSize + (buttonCount - 1) * buttonGap;
            float x = center.x - rowWidth / 2f;

            DrawIconButton(new Rect(x, buttonY, buttonSize, buttonSize), TexButton.Reveal, "CC_Radial_Back".Translate(), 0.99f, true, back);
            x += buttonSize + buttonGap;

            if (infoDef != null) {
                DrawIconButton(
                    new Rect(x, buttonY, buttonSize, buttonSize),
                    TexButton.Info,
                    "CC_Radial_Info".Translate(),
                    0.7f,
                    false,
                    () => Find.WindowStack.Add(new Dialog_InfoCard(infoDef))
                );
                x += buttonSize + buttonGap;
            }

            DrawIconButton(new Rect(x, buttonY, buttonSize, buttonSize), TexButton.CloseXSmall, "CC_Radial_Close".Translate(), 0.55f, false, close);
        } else if (!canFlare) {
            // Quick mode draws no buttons, so this hint takes the button row.
            UIText.EllipsisLabel(
                ChordRow(center, ButtonRowY, tinyH),
                "CC_Radial_Hint_Release".Translate(),
                GameFont.Tiny,
                TextAnchor.MiddleCenter,
                DockPalette.GroupLabel
            );
        }
    }

    // Trims text until its wrapped height fits maxLines, appending an ellipsis.
    // Truncate measures a single line, which does not predict how many lines the
    // text wraps to, so long descriptions would otherwise spill past their rect.
    private static string FitToLines(string text, float width, int maxLines) {
        float lineHeight = Text.LineHeightOf(GameFont.Tiny);
        float maxHeight = lineHeight * maxLines + 1f;
        if (Text.CalcHeight(text, width) <= maxHeight) return text;

        int low = 0;
        int high = text.Length;
        while (low < high) {
            int mid = (low + high + 1) / 2;
            if (Text.CalcHeight(text.Substring(0, mid) + "...", width) <= maxHeight) low = mid;
            else high = mid - 1;
        }

        return low <= 0 ? string.Empty : text.Substring(0, low).TrimEnd() + "...";
    }

    // Draws the action title, wrapping to a second line when it will not fit,
    // and returns how many lines it used so the rows below can shift down.
    private static int DrawWrappedTitle(Vector2 center, float y, float lineHeight, string title, Color color) {
        Rect oneLine = ChordRow(center, y, lineHeight);
        float needed;
        using (new TextBlock(GameFont.Medium)) {
            needed = Text.CalcSize(title).x;
        }

        if (needed <= oneLine.width) {
            UIText.EllipsisLabel(oneLine, title, GameFont.Medium, TextAnchor.MiddleCenter, color);
            return 1;
        }

        Rect twoLines = ChordRow(center, y, lineHeight * 2f);
        using (new TextBlock(GameFont.Medium, TextAnchor.UpperCenter, color)) {
            Widgets.Label(twoLines, title);
        }

        return 2;
    }

    private static bool ShiftHeld() {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private static void DrawIconButton(Rect rect, Texture2D icon, string tooltip, float iconScale, bool mirrored, Action onClick) {
        bool hovered = Mouse.IsOver(rect);
        Widgets.DrawBoxSolid(rect, new Color(1f, 1f, 1f, hovered ? 0.12f : 0.05f));

        float targetHeight = rect.height * iconScale;
        float aspect = icon.height > 0 ? icon.width / (float)icon.height : 1f;
        float drawHeight = targetHeight;
        float drawWidth = targetHeight * aspect;

        Rect iconRect = new Rect(
            rect.center.x - drawWidth / 2f,
            rect.center.y - drawHeight / 2f,
            drawWidth,
            drawHeight
        );

        Color prev = GUI.color;
        GUI.color = hovered ? Color.white : new Color(0.78f, 0.80f, 0.82f);
        if (mirrored) {
            // Flipped through tex coords rather than ScaleAroundPivot: that pivot is in the
            // wrong space at any UI scale above 1 and throws the icon clear of the wheel.
            GUI.DrawTextureWithTexCoords(iconRect, icon, new Rect(1f, 0f, -1f, 1f));
        } else {
            GUI.DrawTexture(iconRect, icon);
        }

        GUI.color = prev;

        TooltipHandler.TipRegion(rect, tooltip);
        Verse.Sound.MouseoverSounds.DoRegion(rect);
        if (Widgets.ButtonInvisible(rect)) onClick();
    }

    private static Rect ChordRow(Vector2 center, float dyTop, float height) {
        float r = RadialLayout.CenterRadius - 6f;
        float worstDy = Mathf.Max(Mathf.Abs(dyTop), Mathf.Abs(dyTop + height));
        float half = worstDy >= r ? 0f : Mathf.Sqrt(r * r - worstDy * worstDy);
        return new Rect(center.x - half, center.y + dyTop, half * 2f, height);
    }

    private static string BuildMetaLine(RadialLeaf leaf) {
        // The cost hint doubles as the burn rate, which is shown inside the
        // reserve bar; only surface it here when there is no bar to carry it.
        string meta = leaf.ReserveFraction.HasValue || leaf.CostHint == null ? string.Empty : leaf.CostHint;
        if (leaf.CooldownTicksRemaining > 0) {
            float seconds = leaf.CooldownTicksRemaining / (float)GenTicks.TicksPerRealSecond;
            string cd = "CC_Radial_Cooldown".Translate(seconds.ToString("F1").Named("SECONDS"));
            meta = meta.Length > 0 ? meta + " - " + cd : cd;
        }

        return meta;
    }

    private static string BuildRateLine(RadialLeaf leaf) {
        return leaf.CostHint ?? string.Empty;
    }
}
