using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Playground;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Cosmere.Lightweave.Playground.PlaygroundChips;

namespace Cosmere.Lightweave.Feedback;

[Doc(
    Id = "badge",
    Summary = "Small status pill with a label and optional glyphs.",
    WhenToUse = "Tag rows or cards with a state, count, or category.",
    SourcePath = "CosmereCore/CosmereCore/Lightweave/Feedback/Badge.cs"
)]
public static class Badge {
    public static LightweaveNode Create(
        [DocParam("Display text. Rendered uppercase.")]
        string text,
        [DocParam("Color variant for the badge surface and foreground.")]
        BadgeVariant variant = BadgeVariant.Neutral,
        [DocParam("Optional leading glyph node.")]
        LightweaveNode? leading = null,
        [DocParam("Optional trailing glyph node.")]
        LightweaveNode? trailing = null,
        [DocParam("Click handler for the entire badge.")]
        Action? onClick = null,
        [DocParam("Click handler scoped to the trailing glyph hit area.")]
        Action? onTrailingClick = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Badge:{variant}", line, file);
        node.PreferredHeight = new Rem(1.25f).ToPixels();

        if (leading != null) {
            node.Children.Add(leading);
        }
        if (trailing != null) {
            node.Children.Add(trailing);
        }

        // NOTE: Small-caps is approximated via uppercasing; fine for Latin, may need revisiting for non-Latin scripts.
        string display = text.ToUpperInvariant();

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;
            Event e = Event.current;

            BackgroundSpec bg = new BackgroundSpec.Solid(BadgeVariants.Background(variant));
            ThemeSlot? borderSlot = BadgeVariants.Border(variant);
            BorderSpec? border = borderSlot.HasValue
                ? BorderSpec.All(new Rem(1f / 16f), borderSlot.Value)
                : null;
            RadiusSpec radius = RadiusSpec.All(new Rem(999f));

            PaintBox.Draw(rect, bg, border, radius);

            float padPx = new Rem(0.5f).ToPixels();
            float gapPx = new Rem(0.25f).ToPixels();
            float iconSize = new Rem(0.875f).ToPixels();
            float trailingHitSize = new Rem(1f).ToPixels();

            float leftEdge = rect.x + padPx;
            float rightEdge = rect.xMax - padPx;

            Rect? trailingHitRect = null;
            if (trailing != null) {
                float trailingX = rtl ? leftEdge : rightEdge - trailingHitSize;
                trailingHitRect = new Rect(
                    trailingX,
                    rect.y + (rect.height - trailingHitSize) / 2f,
                    trailingHitSize,
                    trailingHitSize
                );
                Rect iconRect = new Rect(
                    trailingHitRect.Value.x + (trailingHitSize - iconSize) / 2f,
                    trailingHitRect.Value.y + (trailingHitSize - iconSize) / 2f,
                    iconSize,
                    iconSize
                );
                trailing.MeasuredRect = iconRect;
                if (rtl) {
                    leftEdge = trailingHitRect.Value.xMax + gapPx;
                } else {
                    rightEdge = trailingHitRect.Value.x - gapPx;
                }
            }

            if (leading != null) {
                float leadingX = rtl ? rightEdge - iconSize : leftEdge;
                Rect iconRect = new Rect(
                    leadingX,
                    rect.y + (rect.height - iconSize) / 2f,
                    iconSize,
                    iconSize
                );
                leading.MeasuredRect = iconRect;
                if (rtl) {
                    rightEdge = leadingX - gapPx;
                } else {
                    leftEdge = leadingX + iconSize + gapPx;
                }
            }

            Rect textRect = new Rect(leftEdge, rect.y, Mathf.Max(0f, rightEdge - leftEdge), rect.height);

            Font font = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
            GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleCenter;

            Color savedColor = GUI.color;
            GUI.color = theme.GetColor(BadgeVariants.Foreground(variant));
            GUI.Label(RectSnap.Snap(textRect), display, style);
            GUI.color = savedColor;

            if (onTrailingClick != null && trailingHitRect.HasValue) {
                Rect hit = trailingHitRect.Value;
                bool hovering = hit.Contains(e.mousePosition);
                bool pressed = hovering && UnityEngine.Input.GetMouseButton(0);
                if (hovering) {
                    Color overlay = pressed
                        ? new Color(0f, 0f, 0f, 0.35f)
                        : new Color(1f, 1f, 1f, 0.22f);
                    PaintBox.Draw(
                        hit,
                        new BackgroundSpec.Solid(overlay),
                        null,
                        RadiusSpec.All(new Rem(999f))
                    );
                    MouseoverSounds.DoRegion(hit);
                }
                if (e.type == EventType.MouseUp && e.button == 0 && hovering) {
                    onTrailingClick.Invoke();
                    e.Use();
                }
            }

            if (onClick != null) {
                Rect clickRect = trailingHitRect.HasValue && onTrailingClick != null
                    ? ExcludeTrailing(rect, trailingHitRect.Value, rtl)
                    : rect;
                if (clickRect.Contains(e.mousePosition)) {
                    MouseoverSounds.DoRegion(clickRect);
                }
                if (e.type == EventType.MouseUp && e.button == 0 && clickRect.Contains(e.mousePosition)) {
                    onClick.Invoke();
                    e.Use();
                }
            }

            paintChildren();
        };

        return node;
    }

    [DocVariant("CC_Playground_Feedback_Badge_Neutral")]
    public static DocSample DocsNeutral() {
        return new DocSample(Badge.Create((string)"CC_Playground_Feedback_Badge_Neutral".Translate(), BadgeVariant.Neutral));
    }

    [DocVariant("CC_Playground_Feedback_Badge_Accent", Order = 1)]
    public static DocSample DocsAccent() {
        return new DocSample(Badge.Create((string)"CC_Playground_Feedback_Badge_Accent".Translate(), BadgeVariant.Accent));
    }

    [DocVariant("CC_Playground_Feedback_Badge_Warning", Order = 2)]
    public static DocSample DocsWarning() {
        return new DocSample(Badge.Create((string)"CC_Playground_Feedback_Badge_Warning".Translate(), BadgeVariant.Warning));
    }

    [DocVariant("CC_Playground_Feedback_Badge_Danger", Order = 3)]
    public static DocSample DocsDanger() {
        return new DocSample(Badge.Create((string)"CC_Playground_Feedback_Badge_Danger".Translate(), BadgeVariant.Danger));
    }

    [DocVariant("CC_Playground_Feedback_Badge_Success", Order = 4)]
    public static DocSample DocsSuccess() {
        return new DocSample(Badge.Create((string)"CC_Playground_Feedback_Badge_Success".Translate(), BadgeVariant.Success));
    }

    [DocVariant("CC_Playground_Feedback_Badge_Clickable", Order = 5)]
    public static DocSample DocsClickable() {
        return new DocSample(
            CenterFixed(
                Badge.Create(
                    (string)"CC_Playground_Feedback_Badge_Clickable".Translate(),
                    BadgeVariant.Accent,
                    onClick: () => { }),
                120f,
                24f)
        );
    }

    [DocVariant("CC_Playground_Feedback_Badge_Dismissible", Order = 6)]
    public static DocSample DocsDismissible() {
        return new DocSample(
            CenterFixed(
                Badge.Create(
                    (string)"CC_Playground_Feedback_Badge_Dismissible".Translate(),
                    BadgeVariant.Neutral,
                    trailing: Badge.CloseGlyph(BadgeVariant.Neutral),
                    onTrailingClick: () => { }),
                150f,
                24f)
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(Badge.Create("Storms", BadgeVariant.Accent));
    }

    public static LightweaveNode CloseGlyph(
        BadgeVariant variant = BadgeVariant.Neutral,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("BadgeCloseGlyph", line, file);
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Color color = theme.GetColor(BadgeVariants.Foreground(variant));

            float thickness = Mathf.Max(1f, rect.height * 0.12f);
            float inset = rect.height * 0.18f;
            Rect inner = new Rect(
                rect.x + inset,
                rect.y + inset,
                rect.width - inset * 2f,
                rect.height - inset * 2f
            );

            Color saved = GUI.color;
            GUI.color = color;
            DrawDiagonal(inner, thickness, ascending: true);
            DrawDiagonal(inner, thickness, ascending: false);
            GUI.color = saved;
        };
        return node;
    }

    private static void DrawDiagonal(Rect r, float thickness, bool ascending) {
        Matrix4x4 saved = GUI.matrix;
        Vector2 pivot = new Vector2(r.center.x, r.center.y);
        float angle = ascending ? -45f : 45f;
        GUIUtility.RotateAroundPivot(angle, pivot);
        float length = Mathf.Sqrt(r.width * r.width + r.height * r.height);
        Rect line = new Rect(pivot.x - length / 2f, pivot.y - thickness / 2f, length, thickness);
        GUI.DrawTexture(line, Texture2D.whiteTexture);
        GUI.matrix = saved;
    }

    private static Rect ExcludeTrailing(Rect badgeRect, Rect trailing, bool rtl) {
        if (rtl) {
            float startX = trailing.xMax;
            return new Rect(startX, badgeRect.y, Mathf.Max(0f, badgeRect.xMax - startX), badgeRect.height);
        }
        float endX = trailing.x;
        return new Rect(badgeRect.x, badgeRect.y, Mathf.Max(0f, endX - badgeRect.x), badgeRect.height);
    }
}
