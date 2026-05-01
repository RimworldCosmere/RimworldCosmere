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

[Doc(
    Id = "tabs",
    Summary = "Horizontal tab bar that switches the body region.",
    WhenToUse = "Move between sibling views inside one window.",
    SourcePath = "CosmereCore/CosmereCore/Lightweave/Navigation/Tabs.cs"
)]
public static class Tabs {
    public static LightweaveNode Create<T>(
        [DocParam("Currently selected tab value.")]
        T value,
        [DocParam("Tab values in display order.")]
        IReadOnlyList<T> items,
        [DocParam("Maps a tab value to the visible label.")]
        Func<T, string> labelFn,
        [DocParam("Invoked when the user picks a different tab.")]
        Action<T> onChange,
        [DocParam("Builds the body node for the active tab.")]
        Func<T, LightweaveNode> bodyFn,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Tabs<{typeof(T).Name}>", line, file);
        LightweaveNode bodyNode = bodyFn(value);
        node.Children.Add(bodyNode);

        float barHeight = new Rem(2.5f).ToPixels();
        float dividerThickness = new Rem(1f / 16f).ToPixels();
        float chromeHeight = barHeight + dividerThickness;

        if (bodyNode.Measure != null) {
            node.Measure = width => chromeHeight + bodyNode.Measure(width);
        } else if (bodyNode.PreferredHeight.HasValue) {
            node.PreferredHeight = chromeHeight + bodyNode.PreferredHeight.Value;
        }

        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            float underlineThickness = new Rem(2f / 16f).ToPixels();
            float tabGap = new Rem(0.25f).ToPixels();
            float tabPadding = SpacingScale.Md.ToPixels();

            Rect barRect = new Rect(rect.x, rect.y, rect.width, barHeight);
            Rect dividerRect = new Rect(rect.x, barRect.yMax, rect.width, dividerThickness);
            Rect bodyRect = new Rect(
                rect.x,
                dividerRect.yMax,
                rect.width,
                Mathf.Max(0f, rect.yMax - dividerRect.yMax)
            );

            Font font = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
            GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleCenter;

            int count = items.Count;
            float[] widths = new float[count];
            for (int i = 0; i < count; i++) {
                string label = labelFn(items[i]);
                Vector2 labelSize = style.CalcSize(new GUIContent(label));
                widths[i] = labelSize.x + tabPadding * 2f;
            }

            Event e = Event.current;
            Color savedColor = GUI.color;

            float cursor = rtl ? barRect.xMax : barRect.x;
            for (int i = 0; i < count; i++) {
                T item = items[i];
                bool active = EqualityComparer<T>.Default.Equals(item, value);
                float tabWidth = widths[i];

                Rect tabRect;
                if (rtl) {
                    tabRect = new Rect(cursor - tabWidth, barRect.y, tabWidth, barRect.height);
                    cursor -= tabWidth + tabGap;
                } else {
                    tabRect = new Rect(cursor, barRect.y, tabWidth, barRect.height);
                    cursor += tabWidth + tabGap;
                }

                bool hovering = tabRect.Contains(e.mousePosition);
                if (!active) {
                    PaintBox.DrawHighlightIfMouseover(tabRect, RadiusSpec.Top(new Rem(0.25f)));
                    MouseoverSounds.DoRegion(tabRect);
                }

                ThemeSlot textSlot = active || hovering ? ThemeSlot.TextPrimary : ThemeSlot.TextSecondary;

                GUI.color = theme.GetColor(textSlot);
                GUI.Label(RectSnap.Snap(tabRect), labelFn(item), style);
                GUI.color = savedColor;

                if (active) {
                    Rect underlineRect = new Rect(
                        tabRect.x,
                        tabRect.yMax - underlineThickness,
                        tabRect.width,
                        underlineThickness
                    );
                    PaintBox.Draw(underlineRect, new BackgroundSpec.Solid(ThemeSlot.SurfaceAccent), null, null);
                }

                if (e.type == EventType.MouseUp && e.button == 0 && tabRect.Contains(e.mousePosition)) {
                    onChange?.Invoke(item);
                    e.Use();
                }
            }

            PaintBox.Draw(dividerRect, new BackgroundSpec.Solid(ThemeSlot.BorderSubtle), null, null);

            bodyNode.MeasuredRect = bodyRect;
            LightweaveRoot.PaintSubtree(bodyNode, bodyRect);
        };

        return node;
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        Hooks.Hooks.StateHandle<string> selected = Hooks.Hooks.UseState<string>("general");

        string[] tabs = new[] { "general", "combat", "storage" };
        return new DocSample(
            Tabs.Create(
                selected.Value,
                tabs,
                v => v switch {
                    "combat" => (string)"CC_Playground_Navigation_Tabs_Combat".Translate(),
                    "storage" => (string)"CC_Playground_Navigation_Tabs_Storage".Translate(),
                    _ => (string)"CC_Playground_Navigation_Tabs_General".Translate(),
                },
                v => selected.Set(v),
                v => Caption.Create(
                    v switch {
                        "combat" => (string)"CC_Playground_Navigation_Tabs_Body_Combat".Translate(),
                        "storage" => (string)"CC_Playground_Navigation_Tabs_Body_Storage".Translate(),
                        _ => (string)"CC_Playground_Navigation_Tabs_Body_General".Translate(),
                    }
                )
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        Hooks.Hooks.StateHandle<string> selected = Hooks.Hooks.UseState<string>("general");
        string[] tabs = new[] { "general", "combat", "storage" };
        return new DocSample(
            Tabs.Create(
                selected.Value,
                tabs,
                v => v,
                v => selected.Set(v),
                v => Caption.Create(v)
            )
        );
    }
}