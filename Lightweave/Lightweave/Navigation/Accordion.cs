using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Hooks;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Cosmere.Lightweave.Typography.Typography;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Navigation;

public sealed record AccordionItem(
    string Id,
    string Header,
    LightweaveNode Content,
    float ContentHeight
);

[Doc(
    Id = "accordion",
    Summary = "Stacked collapsible panels in single or multi-expand mode.",
    WhenToUse = "Reveal long-form content in sections without leaving the surface.",
    SourcePath = "Lightweave/Lightweave/Navigation/Accordion.cs",
    PreferredVariantHeight = 260f
)]
public static class Accordion {
    private const float HeaderHeight = 40f;
    private const float ExpandDurationSeconds = 0.18f;
    private static readonly Func<float, float> EaseOutCubic = t => 1f - Mathf.Pow(1f - t, 3f);

    public static LightweaveNode Create(
        [DocParam("Section definitions in display order.")]
        IReadOnlyList<AccordionItem> items,
        [DocParam("Set of section ids currently expanded.")]
        HashSet<string> expandedIds,
        [DocParam("Invoked with the section id when toggled.")]
        Action<string> onToggle,
        [DocParam("Single-open or multi-open behavior.")]
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
            GUIStyle headerStyle = GuiStyleCache.GetOrCreate(headerFont, headerFontSize, FontStyle.Bold);
            headerStyle.alignment = rtl ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            headerStyle.wordWrap = false;

            Font chevronFont = theme.GetFont(FontRole.Body);
            int chevronFontSize = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
            GUIStyle chevronStyle = GuiStyleCache.GetOrCreate(chevronFont, chevronFontSize);
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

                BackgroundSpec headerBg = BackgroundSpec.Of(ThemeSlot.SurfaceRaised);
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
                    PaintBox.DrawHighlight(headerRect, RadiusSpec.All(RadiusScale.Sm), true);
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
                }
                else {
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

                    BackgroundSpec panelBg = BackgroundSpec.Of(ThemeSlot.SurfacePrimary);
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
                    onToggle?.Invoke(item.Id);
                    e.Use();
                }
            }
        };

        return node;
    }

    public static float MeasureHeight(IReadOnlyList<AccordionItem> items, HashSet<string> expandedIds) {
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

    private static List<AccordionItem> BuildSampleItems() {
        LightweaveNode overviewBody = Text.Create(
            (string)"CL_Playground_accordion_Body_Overview".Translate(),
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextPrimary,
            wrap: true
        );
        LightweaveNode stormlightBody = Text.Create(
            (string)"CL_Playground_accordion_Body_Stormlight".Translate(),
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextPrimary,
            wrap: true
        );
        LightweaveNode sprenBody = Text.Create(
            (string)"CL_Playground_accordion_Body_Spren".Translate(),
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextPrimary,
            wrap: true
        );

        return new List<AccordionItem> {
            new AccordionItem(
                "overview",
                (string)"CL_Playground_accordion_Header_Overview".Translate(),
                overviewBody,
                56f
            ),
            new AccordionItem(
                "stormlight",
                (string)"CL_Playground_accordion_Header_Stormlight".Translate(),
                stormlightBody,
                64f
            ),
            new AccordionItem(
                "spren",
                (string)"CL_Playground_accordion_Header_Spren".Translate(),
                sprenBody,
                64f
            ),
        };
    }

    [DocVariant("CL_Playground_accordion_Mode_Single")]
    public static DocSample DocsSingle() {
        return new DocSample(() => {
            Hooks.Hooks.StateHandle<HashSet<string>> open =
                Hooks.Hooks.UseState<HashSet<string>>(new HashSet<string> { "overview" });
            return Accordion.Create(
                BuildSampleItems(),
                open.Value,
                id => {
                    HashSet<string> next = open.Value.Contains(id)
                        ? new HashSet<string>()
                        : new HashSet<string> { id };
                    open.Set(next);
                }
            );
        });
    }

    [DocVariant("CL_Playground_accordion_Mode_Multi")]
    public static DocSample DocsMulti() {
        return new DocSample(() => {
            Hooks.Hooks.StateHandle<HashSet<string>> open =
                Hooks.Hooks.UseState<HashSet<string>>(new HashSet<string> { "stormlight", "spren" });
            return Accordion.Create(
                BuildSampleItems(),
                open.Value,
                id => {
                    HashSet<string> next = new HashSet<string>(open.Value);
                    if (!next.Add(id)) {
                        next.Remove(id);
                    }

                    open.Set(next);
                },
                AccordionMode.Multi
            );
        });
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => {
            Hooks.Hooks.RefHandle<HashSet<string>> expanded =
                Hooks.Hooks.UseRef(new HashSet<string> { "overview" });
            return Accordion.Create(
                BuildSampleItems(),
                expanded.Current,
                _ => { }
            );
        });
    }
}