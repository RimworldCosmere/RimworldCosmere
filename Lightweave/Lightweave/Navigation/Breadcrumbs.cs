using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Navigation;

[Doc(
    Id = "breadcrumbs",
    Summary = "Inline path with chevrons that collapses on overflow.",
    WhenToUse = "Show ancestry through a hierarchy users can navigate back through.",
    SourcePath = "Lightweave/Lightweave/Navigation/Breadcrumbs.cs"
)]
public static class Breadcrumbs {
    private const string Ellipsis = "...";
    private static readonly Rem RowHeight = new Rem(1.5f);
    private static readonly Rem LabelSize = new Rem(0.875f);

    public static LightweaveNode Create(
        [DocParam("Ordered crumb labels from root to current.")]
        IReadOnlyList<string> crumbs,
        [DocParam("Invoked when an earlier crumb is clicked.")]
        Action<int>? onNavigate = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Breadcrumbs", line, file);
        node.PreferredHeight = RowHeight.ToPixels();
        node.Paint = (rect, _) => {
            if (crumbs == null || crumbs.Count == 0) {
                return;
            }

            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            Font font = theme.GetFont(FontRole.Body);
            int pixelSize = Mathf.RoundToInt(LabelSize.ToFontPx());
            GUIStyle style = GuiStyleCache.Get(font, pixelSize);
            style.alignment = TextAnchor.MiddleLeft;

            string chevronGlyph = rtl ? "‹" : "›";
            float gapPx = SpacingScale.Xs.ToPixels();
            float rowHeight = RowHeight.ToPixels();
            float rowY = rect.y + (rect.height - rowHeight) * 0.5f;

            int count = crumbs.Count;
            float[] labelWidths = new float[count];
            for (int i = 0; i < count; i++) {
                string crumbText = crumbs[i] ?? string.Empty;
                Vector2 size = style.CalcSize(new GUIContent(crumbText));
                labelWidths[i] = size.x;
            }

            Vector2 chevronSize = style.CalcSize(new GUIContent(chevronGlyph));
            float chevronWidth = chevronSize.x;
            Vector2 ellipsisSize = style.CalcSize(new GUIContent(Ellipsis));
            float ellipsisWidth = ellipsisSize.x;

            bool[] visible = new bool[count];
            for (int i = 0; i < count; i++) {
                visible[i] = true;
            }

            bool showEllipsis = false;

            if (count > 1) {
                float total = TotalWidth(labelWidths, visible, count, chevronWidth, gapPx, showEllipsis, ellipsisWidth);
                int removeIndex = 1;
                while (total > rect.width && removeIndex < count - 1) {
                    visible[removeIndex] = false;
                    showEllipsis = true;
                    removeIndex++;
                    total = TotalWidth(labelWidths, visible, count, chevronWidth, gapPx, showEllipsis, ellipsisWidth);
                }
            }

            int lastVisibleIndex = count - 1;

            float cursor = rtl ? rect.xMax : rect.x;
            bool firstDrawn = true;
            bool ellipsisDrawn = false;

            for (int i = 0; i < count; i++) {
                if (!visible[i]) {
                    if (!ellipsisDrawn && showEllipsis) {
                        if (!firstDrawn) {
                            cursor = DrawChevron(
                                cursor,
                                rowY,
                                rowHeight,
                                chevronWidth,
                                chevronGlyph,
                                style,
                                theme,
                                rtl,
                                gapPx
                            );
                        }

                        cursor = DrawEllipsis(cursor, rowY, rowHeight, ellipsisWidth, style, theme, rtl, gapPx);
                        firstDrawn = false;
                        ellipsisDrawn = true;
                    }

                    continue;
                }

                if (!firstDrawn) {
                    cursor = DrawChevron(cursor, rowY, rowHeight, chevronWidth, chevronGlyph, style, theme, rtl, gapPx);
                }

                string crumbText = crumbs[i] ?? string.Empty;
                float labelWidth = labelWidths[i];
                bool isLast = i == lastVisibleIndex;

                Rect labelRect;
                if (rtl) {
                    labelRect = new Rect(cursor - labelWidth, rowY, labelWidth, rowHeight);
                    cursor = labelRect.x - gapPx;
                } else {
                    labelRect = new Rect(cursor, rowY, labelWidth, rowHeight);
                    cursor = labelRect.xMax + gapPx;
                }

                DrawCrumb(labelRect, crumbText, i, isLast, onNavigate, style, theme);
                firstDrawn = false;
            }
        };
        return node;
    }

    private static float TotalWidth(
        float[] labelWidths,
        bool[] visible,
        int count,
        float chevronWidth,
        float gapPx,
        bool showEllipsis,
        float ellipsisWidth
    ) {
        float total = 0f;
        int visibleCount = 0;
        for (int i = 0; i < count; i++) {
            if (visible[i]) {
                total += labelWidths[i];
                visibleCount++;
            }
        }

        if (showEllipsis) {
            total += ellipsisWidth;
            visibleCount++;
        }

        if (visibleCount > 1) {
            int separators = visibleCount - 1;
            total += separators * (chevronWidth + gapPx * 2f);
        }

        return total;
    }

    private static float DrawChevron(
        float cursor,
        float rowY,
        float rowHeight,
        float chevronWidth,
        string chevronGlyph,
        GUIStyle style,
        Theme.Theme theme,
        bool rtl,
        float gapPx
    ) {
        Rect chevronRect;
        float next;
        if (rtl) {
            chevronRect = new Rect(cursor - chevronWidth, rowY, chevronWidth, rowHeight);
            next = chevronRect.x - gapPx;
        } else {
            chevronRect = new Rect(cursor, rowY, chevronWidth, rowHeight);
            next = chevronRect.xMax + gapPx;
        }

        Color saved = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextMuted);
        GUI.Label(RectSnap.Snap(chevronRect), chevronGlyph, style);
        GUI.color = saved;
        return next;
    }

    private static float DrawEllipsis(
        float cursor,
        float rowY,
        float rowHeight,
        float ellipsisWidth,
        GUIStyle style,
        Theme.Theme theme,
        bool rtl,
        float gapPx
    ) {
        Rect ellipsisRect;
        float next;
        if (rtl) {
            ellipsisRect = new Rect(cursor - ellipsisWidth, rowY, ellipsisWidth, rowHeight);
            next = ellipsisRect.x - gapPx;
        } else {
            ellipsisRect = new Rect(cursor, rowY, ellipsisWidth, rowHeight);
            next = ellipsisRect.xMax + gapPx;
        }

        Color saved = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextMuted);
        GUI.Label(RectSnap.Snap(ellipsisRect), Ellipsis, style);
        GUI.color = saved;
        return next;
    }

    private static void DrawCrumb(
        Rect labelRect,
        string text,
        int index,
        bool isLast,
        Action<int>? onNavigate,
        GUIStyle style,
        Theme.Theme theme
    ) {
        Event e = Event.current;
        bool interactive = !isLast;
        bool hovering = interactive && labelRect.Contains(e.mousePosition);

        if (hovering) {
            PaintBox.DrawHighlight(labelRect, RadiusSpec.All(new Rem(0.25f)), true);
        }

        ThemeSlot slot;
        if (isLast) {
            slot = ThemeSlot.TextPrimary;
        } else if (hovering) {
            slot = ThemeSlot.TextPrimary;
        } else {
            slot = ThemeSlot.TextMuted;
        }

        Color saved = GUI.color;
        GUI.color = theme.GetColor(slot);
        GUI.Label(RectSnap.Snap(labelRect), text, style);
        GUI.color = saved;

        if (interactive && e.type == EventType.MouseUp && e.button == 0 && labelRect.Contains(e.mousePosition)) {
            onNavigate?.Invoke(index);
            e.Use();
        }
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        string[] path = new[] {
            (string)"CC_Playground_Breadcrumbs_Crumb_Worlds".Translate(),
            (string)"CC_Playground_Breadcrumbs_Crumb_Roshar".Translate(),
            (string)"CC_Playground_Breadcrumbs_Crumb_ShatteredPlains".Translate(),
        };
        return new DocSample(Breadcrumbs.Create(path));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        string[] path = new[] { "Worlds", "Roshar", "Shattered Plains" };
        return new DocSample(Breadcrumbs.Create(path));
    }
}