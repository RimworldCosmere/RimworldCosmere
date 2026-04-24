using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Lightweave.Navigation;

public sealed record AccordionItem(
    string Id,
    string Header,
    LightweaveNode Content,
    float ContentHeight
);

public static class Accordion {
    private const float HeaderHeight = 40f;
    private const float ExpandDurationSeconds = 0.18f;
    private static readonly Func<float, float> EaseOutCubic = t => 1f - Mathf.Pow(1f - t, 3f);

    public static LightweaveNode Create(
        IReadOnlyList<AccordionItem> items,
        ISet<string> expandedIds,
        Action<string> onToggle,
        AccordionMode mode = AccordionMode.Single,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Accordion:{mode}", line, file);

        for (int i = 0; i < items.Count; i++) {
            node.Children.Add(items[i].Content);
        }

        node.Measure = _ => MeasureHeight(items, expandedIds);

        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            float borderPx = new Rem(1f / 16f).ToPixels();
            float headerPadX = SpacingScale.Md.ToPixels();
            float chevronSize = new Rem(0.75f).ToPixels();
            float contentPadX = SpacingScale.Md.ToPixels();
            float contentPadY = SpacingScale.Sm.ToPixels();

            Font headerFont = theme.GetFont(FontRole.BodyBold);
            int headerFontSize = Mathf.RoundToInt(new Rem(0.9375f).ToFontPx());
            GUIStyle headerStyle = GuiStyleCache.Get(headerFont, headerFontSize, FontStyle.Bold);
            headerStyle.alignment = rtl ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            headerStyle.wordWrap = false;

            Font chevronFont = theme.GetFont(FontRole.Body);
            int chevronFontSize = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
            GUIStyle chevronStyle = GuiStyleCache.Get(chevronFont, chevronFontSize);
            chevronStyle.alignment = TextAnchor.MiddleCenter;
            chevronStyle.wordWrap = false;

            Event e = Event.current;
            float cursorY = rect.y;
            Color savedColor = GUI.color;

            for (int i = 0; i < items.Count; i++) {
                AccordionItem item = items[i];
                bool expanded = expandedIds.Contains(item.Id);

                float progress = UseAnim.Animate(
                    expanded ? 1f : 0f,
                    ExpandDurationSeconds,
                    EaseOutCubic,
                    i,
                    file + ":" + line + "#acc:" + item.Id
                );
                float revealHeight = item.ContentHeight * progress;

                bool isFirst = i == 0;
                bool isLast = i == items.Count - 1;

                Rect headerRect = new Rect(rect.x, cursorY, rect.width, HeaderHeight);

                BackgroundSpec headerBg = new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised);
                ThemeSlot headerBorderSlot = ThemeSlot.BorderDefault;
                BorderSpec headerBorder = new BorderSpec(
                    isFirst ? new Rem(1f / 16f) : null,
                    Left: new Rem(1f / 16f),
                    Right: new Rem(1f / 16f),
                    Bottom: new Rem(1f / 16f),
                    Color: headerBorderSlot
                );

                PaintBox.Draw(headerRect, headerBg, headerBorder, null);

                bool hovered = headerRect.Contains(e.mousePosition);
                if (hovered) {
                    PaintBox.DrawHighlight(headerRect, RadiusSpec.All(new Rem(0.25f)), true);
                }

                MouseoverSounds.DoRegion(headerRect);

                float chevronX = rtl
                    ? headerRect.x + headerPadX
                    : headerRect.xMax - headerPadX - chevronSize;
                Rect chevronRect = new Rect(
                    chevronX,
                    headerRect.y + (headerRect.height - chevronSize) / 2f,
                    chevronSize,
                    chevronSize
                );

                Rect labelRect;
                if (rtl) {
                    float labelX = chevronRect.xMax + SpacingScale.Xs.ToPixels();
                    labelRect = new Rect(
                        labelX,
                        headerRect.y,
                        headerRect.xMax - headerPadX - labelX,
                        headerRect.height
                    );
                } else {
                    labelRect = new Rect(
                        headerRect.x + headerPadX,
                        headerRect.y,
                        chevronRect.x - SpacingScale.Xs.ToPixels() - (headerRect.x + headerPadX),
                        headerRect.height
                    );
                }

                GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
                GUI.Label(RectSnap.Snap(labelRect), item.Header, headerStyle);

                DrawChevron(chevronRect, theme, progress, rtl);
                GUI.color = savedColor;

                cursorY = headerRect.yMax;

                if (revealHeight > 0.5f) {
                    Rect panelRect = new Rect(rect.x, cursorY, rect.width, revealHeight);

                    BackgroundSpec panelBg = new BackgroundSpec.Solid(ThemeSlot.SurfacePrimary);
                    BorderSpec panelBorder = new BorderSpec(
                        Left: new Rem(1f / 16f),
                        Right: new Rem(1f / 16f),
                        Bottom: isLast ? new Rem(1f / 16f) : null,
                        Color: headerBorderSlot
                    );
                    PaintBox.Draw(panelRect, panelBg, panelBorder, null);

                    Rect innerRect = new Rect(
                        panelRect.x + contentPadX,
                        panelRect.y + contentPadY,
                        Mathf.Max(0f, panelRect.width - contentPadX * 2f),
                        Mathf.Max(0f, panelRect.height - contentPadY * 2f)
                    );

                    GUI.BeginClip(panelRect);
                    Rect clippedInner = new Rect(
                        contentPadX,
                        contentPadY - (item.ContentHeight - revealHeight),
                        innerRect.width,
                        item.ContentHeight - contentPadY * 2f
                    );
                    item.Content.MeasuredRect = clippedInner;
                    LightweaveRoot.PaintSubtree(item.Content, clippedInner);
                    GUI.EndClip();

                    cursorY = panelRect.yMax;
                }

                if (e.type == EventType.MouseUp && e.button == 0 && headerRect.Contains(e.mousePosition)) {
                    if (mode == AccordionMode.Single) {
                        bool wasExpanded = expandedIds.Contains(item.Id);
                        expandedIds.Clear();
                        if (!wasExpanded) {
                            expandedIds.Add(item.Id);
                        }
                    } else {
                        if (!expandedIds.Remove(item.Id)) {
                            expandedIds.Add(item.Id);
                        }
                    }

                    onToggle?.Invoke(item.Id);
                    e.Use();
                }
            }
        };

        return node;
    }

    public static float MeasureHeight(IReadOnlyList<AccordionItem> items, ISet<string> expandedIds) {
        float total = 0f;
        for (int i = 0; i < items.Count; i++) {
            total += HeaderHeight;
            if (expandedIds.Contains(items[i].Id)) {
                total += items[i].ContentHeight;
            }
        }

        return total;
    }

    private static void DrawChevron(Rect rect, Theme.Theme theme, float progress, bool rtl) {
        Matrix4x4 savedMatrix = GUI.matrix;
        Vector2 pivot = new Vector2(rect.x + rect.width / 2f, rect.y + rect.height / 2f);
        float angle = Mathf.Lerp(90f, -90f, progress);
        GUIUtility.RotateAroundPivot(angle, pivot);

        Color savedColor = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextSecondary);
        GUI.DrawTexture(RectSnap.Snap(rect), TexUI.ArrowTexLeft, ScaleMode.ScaleToFit);
        GUI.color = savedColor;

        GUI.matrix = savedMatrix;
    }
}