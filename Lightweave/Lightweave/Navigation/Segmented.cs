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

namespace Cosmere.Lightweave.Navigation;

[Doc(
    Id = "segmented",
    Summary = "Pill-shaped grouped selector for a small set of choices.",
    WhenToUse = "Toggle between 2-5 mutually exclusive filters or modes.",
    SourcePath = "Lightweave/Lightweave/Navigation/Segmented.cs",
    ShowRtl = true
)]
public static class Segmented {
    public static LightweaveNode Create<T>(
        [DocParam("Currently selected segment value.")]
        T value,
        [DocParam("All segment values in display order.")]
        IReadOnlyList<T> items,
        [DocParam("Maps a segment value to its label.")]
        Func<T, string> labelFn,
        [DocParam("Invoked when the user picks a different segment.")]
        Action<T> onChange,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Segmented<{typeof(T).Name}>", line, file);
        node.ApplyStyling("segmented", style, classes, id);
        node.PreferredHeight = new Rem(1.75f).ToPixels();

        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            BackgroundSpec bg = BackgroundSpec.Of(ThemeSlot.SurfaceRaised);
            BorderSpec border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
            RadiusSpec radius = RadiusSpec.All(RadiusScale.Full);
            PaintBox.Draw(rect, bg, border, radius);

            int count = items.Count;
            if (count == 0) {
                return;
            }

            float segmentWidth = rect.width / count;
            float dividerThickness = new Rem(1f / 16f).ToPixels();

            Font inactiveFont = theme.GetFont(FontRole.Body);
            Font activeFont = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
            GUIStyle inactiveStyle = GuiStyleCache.GetOrCreate(inactiveFont, pixelSize);
            inactiveStyle.alignment = TextAnchor.MiddleCenter;
            GUIStyle activeStyle = GuiStyleCache.GetOrCreate(activeFont, pixelSize, FontStyle.Bold);
            activeStyle.alignment = TextAnchor.MiddleCenter;

            int activeIndex = -1;
            for (int i = 0; i < count; i++) {
                if (EqualityComparer<T>.Default.Equals(items[i], value)) {
                    activeIndex = i;
                    break;
                }
            }

            Event e = Event.current;
            Color savedColor = GUI.color;

            for (int i = 0; i < count; i++) {
                int logicalIndex = rtl ? count - 1 - i : i;
                T item = items[logicalIndex];
                bool active = logicalIndex == activeIndex;

                Rect segRect = new Rect(rect.x + i * segmentWidth, rect.y, segmentWidth, rect.height);
                LightweaveHitTracker.Track(segRect);

                if (active) {
                    Rem pill = RadiusSpec.ResolveRem(RadiusScale.Full);
                    bool isFirstLogical = logicalIndex == 0;
                    bool isLastLogical = logicalIndex == count - 1;
                    RadiusSpec activeRadius = new RadiusSpec(
                        TopStart: isFirstLogical ? pill : null,
                        BottomStart: isFirstLogical ? pill : null,
                        TopEnd: isLastLogical ? pill : null,
                        BottomEnd: isLastLogical ? pill : null
                    );
                    PaintBox.Draw(segRect, BackgroundSpec.Of(ThemeSlot.SurfaceAccent), null, activeRadius);
                }

                if (!active) {
                    Rem pill = RadiusSpec.ResolveRem(RadiusScale.Full);
                    bool isFirstHover = logicalIndex == 0;
                    bool isLastHover = logicalIndex == count - 1;
                    RadiusSpec hoverRadius = new RadiusSpec(
                        TopStart: isFirstHover ? pill : null,
                        BottomStart: isFirstHover ? pill : null,
                        TopEnd: isLastHover ? pill : null,
                        BottomEnd: isLastHover ? pill : null
                    );
                    PaintBox.DrawHighlightIfMouseover(segRect, hoverRadius);
                    MouseoverSounds.DoRegion(segRect);
                }

                GUIStyle style = active ? activeStyle : inactiveStyle;
                ThemeSlot textSlot = active ? ThemeSlot.TextOnAccent : ThemeSlot.TextSecondary;
                GUI.color = theme.GetColor(textSlot);
                GUI.Label(RectSnap.Snap(segRect), labelFn(item), style);
                GUI.color = savedColor;

                if (i < count - 1) {
                    int nextLogical = rtl ? count - 2 - i : i + 1;
                    bool adjacentToActive = logicalIndex == activeIndex || nextLogical == activeIndex;
                    if (!adjacentToActive) {
                        Rect dividerRect = new Rect(
                            segRect.xMax - dividerThickness / 2f,
                            segRect.y + segRect.height * 0.25f,
                            dividerThickness,
                            segRect.height * 0.5f
                        );
                        PaintBox.Draw(dividerRect, BackgroundSpec.Of(ThemeSlot.BorderSubtle), null, null);
                    }
                }

                if (e.type == EventType.MouseUp && e.button == 0 && segRect.Contains(e.mousePosition)) {
                    onChange?.Invoke(item);
                    e.Use();
                }
            }
        };

        return node;
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        Hooks.Hooks.StateHandle<string> selected = Hooks.Hooks.UseState<string>("all");

        string[] segments = new[] { "all", "armor", "weapons", "tools" };
        return new DocSample(() => 
            Segmented.Create(
                selected.Value,
                segments,
                v => v switch {
                    "armor" => (string)"CL_Playground_Navigation_Segmented_Armor".Translate(),
                    "weapons" => (string)"CL_Playground_Navigation_Segmented_Weapons".Translate(),
                    "tools" => (string)"CL_Playground_Navigation_Segmented_Tools".Translate(),
                    _ => (string)"CL_Playground_Navigation_Segmented_All".Translate(),
                },
                v => selected.Set(v)
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => {
            Hooks.Hooks.StateHandle<string> selected = Hooks.Hooks.UseState<string>("all");
            string[] segments = new[] { "all", "armor", "weapons" };
            return Segmented.Create(
                selected.Value,
                segments,
                v => v,
                v => selected.Set(v)
            );
        });
    }
}