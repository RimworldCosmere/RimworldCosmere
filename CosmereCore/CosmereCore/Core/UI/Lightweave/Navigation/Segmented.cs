using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;
using Verse.Sound;

namespace Cosmere.Core.UI.Lightweave.Navigation;

public static class Segmented {
    public static LightweaveNode Create<T>(
        T value,
        IReadOnlyList<T> items,
        Func<T, string> labelFn,
        Action<T> onChange,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Segmented<{typeof(T).Name}>", line, file);
        node.PreferredHeight = new Rem(1.75f).ToPixels();

        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            BackgroundSpec bg = new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised);
            BorderSpec border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
            RadiusSpec radius = RadiusSpec.All(new Rem(999f));
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
            GUIStyle inactiveStyle = GuiStyleCache.Get(inactiveFont, pixelSize);
            inactiveStyle.alignment = TextAnchor.MiddleCenter;
            GUIStyle activeStyle = GuiStyleCache.Get(activeFont, pixelSize, FontStyle.Bold);
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
                    Rem pill = new Rem(999f);
                    bool isFirstLogical = logicalIndex == 0;
                    bool isLastLogical = logicalIndex == count - 1;
                    RadiusSpec activeRadius = new RadiusSpec(
                        TopStart: isFirstLogical ? pill : null,
                        BottomStart: isFirstLogical ? pill : null,
                        TopEnd: isLastLogical ? pill : null,
                        BottomEnd: isLastLogical ? pill : null
                    );
                    PaintBox.Draw(segRect, new BackgroundSpec.Solid(ThemeSlot.SurfaceAccent), null, activeRadius);
                }

                if (!active) {
                    Rem pill = new Rem(999f);
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
                        PaintBox.Draw(dividerRect, new BackgroundSpec.Solid(ThemeSlot.BorderSubtle), null, null);
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
}