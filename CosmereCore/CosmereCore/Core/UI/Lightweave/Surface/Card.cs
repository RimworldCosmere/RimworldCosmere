using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Surface;

public static partial class Surface {
    public static class Card {
        public static LightweaveNode Create(
            params LightweaveNode[] children
        ) {
            return CreateInternal(null, children);
        }

        public static LightweaveNode WithPadding(
            Rem padding,
            params LightweaveNode[] children
        ) {
            return CreateInternal(EdgeInsets.All(padding), children);
        }

        private static LightweaveNode CreateInternal(
            EdgeInsets? padding,
            LightweaveNode[] children,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            EdgeInsets pad = padding ?? EdgeInsets.All(SpacingScale.Md);
            BackgroundSpec bg = new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised);
            BorderSpec border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
            RadiusSpec radius = RadiusSpec.All(new Rem(0.5f));

            LightweaveNode node = NodeBuilder.New("Card", line, file);
            for (int i = 0; i < children.Length; i++) {
                node.Children.Add(children[i]);
            }

            float gapPxStatic = new Rem(0.5f).ToPixels();
            int childCount = children.Length;

            bool CanMeasureChildren() {
                for (int i = 0; i < childCount; i++) {
                    if (children[i].Measure == null && !children[i].PreferredHeight.HasValue) {
                        return false;
                    }
                }

                return childCount > 0;
            }

            if (CanMeasureChildren()) {
                float padTopPx = pad.Top?.ToPixels() ?? 0f;
                float padBottomPx = pad.Bottom?.ToPixels() ?? 0f;
                EdgeInsets capturedPad = pad;
                node.Measure = availableWidth => {
                    Direction renderDir = RenderContext.Current.Direction;
                    Rect dummy = new Rect(0f, 0f, availableWidth, 0f);
                    Rect content = capturedPad.Shrink(dummy, renderDir);
                    float innerWidth = content.width;
                    float total = 0f;
                    for (int i = 0; i < childCount; i++) {
                        LightweaveNode child = children[i];
                        total += child.Measure?.Invoke(innerWidth) ?? child.PreferredHeight ?? 0f;
                    }

                    total += gapPxStatic * Mathf.Max(0, childCount - 1);
                    total += padTopPx + padBottomPx;
                    return total;
                };
            }

            node.Paint = (rect, paintChildren) => {
                PaintBox.Draw(rect, bg, border, radius);
                Rect content = pad.Shrink(rect, RenderContext.Current.Direction);

                int count = children.Length;
                if (count == 0) {
                    return;
                }

                float gapPx = new Rem(0.5f).ToPixels();
                float totalGap = gapPx * Mathf.Max(0, count - 1);

                float[] resolvedHeights = new float[count];
                bool[] isFlex = new bool[count];
                float fixedTotal = 0f;
                int flexCount = 0;
                for (int i = 0; i < count; i++) {
                    LightweaveNode child = children[i];
                    float? h = child.Measure?.Invoke(content.width) ?? child.PreferredHeight;
                    if (h.HasValue) {
                        resolvedHeights[i] = h.Value;
                        fixedTotal += h.Value;
                    } else {
                        isFlex[i] = true;
                        flexCount++;
                    }
                }

                float remainingForFlex = Mathf.Max(0f, content.height - fixedTotal - totalGap);
                float flexEach = flexCount > 0 ? remainingForFlex / flexCount : 0f;

                float y = content.y;
                for (int i = 0; i < count; i++) {
                    LightweaveNode child = children[i];
                    float h = isFlex[i] ? flexEach : resolvedHeights[i];
                    child.MeasuredRect = new Rect(content.x, y, content.width, h);
                    y += h + gapPx;
                }

                paintChildren();
            };
            return node;
        }

        public static LightweaveNode Header(
            params LightweaveNode[] children
        ) {
            return HeaderInternal(children);
        }

        private static LightweaveNode HeaderInternal(
            LightweaveNode[] children,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            LightweaveNode node = NodeBuilder.New("Card.Header", line, file);
            for (int i = 0; i < children.Length; i++) {
                node.Children.Add(children[i]);
            }

            float gapPx = new Rem(0.25f).ToPixels();
            float fallbackH = new Rem(1.5f).ToPixels();

            node.Measure = availableWidth => {
                float total = 0f;
                for (int i = 0; i < children.Length; i++) {
                    LightweaveNode child = children[i];
                    total += child.Measure?.Invoke(availableWidth) ?? child.PreferredHeight ?? fallbackH;
                }

                if (children.Length > 1) {
                    total += gapPx * (children.Length - 1);
                }

                return total;
            };

            node.Paint = (rect, paintChildren) => {
                float y = rect.y;
                for (int i = 0; i < children.Length; i++) {
                    LightweaveNode child = children[i];
                    float h = child.Measure?.Invoke(rect.width) ?? child.PreferredHeight ?? fallbackH;
                    child.MeasuredRect = new Rect(rect.x, y, rect.width, h);
                    y += h + gapPx;
                }

                paintChildren();
            };
            return node;
        }

        public static LightweaveNode Title(
            string text,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            LightweaveNode node = NodeBuilder.New($"Card.Title:{text}", line, file);
            node.PreferredHeight = new Rem(1.5f).ToPixels();
            node.Paint = (rect, _) => {
                Theme.Theme theme = RenderContext.Current.Theme;
                Direction dir = RenderContext.Current.Direction;
                Font font = theme.GetFont(FontRole.BodyBold);
                int size = Mathf.RoundToInt(new Rem(1.125f).ToFontPx());
                GUIStyle style = GuiStyleCache.Get(font, size, FontStyle.Bold);
                style.alignment = dir == Direction.Rtl ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
                Color saved = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
                GUI.Label(RectSnap.Snap(rect), text, style);
                GUI.color = saved;
            };
            return node;
        }

        public static LightweaveNode Description(
            string text,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            LightweaveNode node = NodeBuilder.New($"Card.Description:{text}", line, file);
            node.PreferredHeight = new Rem(1.25f).ToPixels();
            node.Paint = (rect, _) => {
                Theme.Theme theme = RenderContext.Current.Theme;
                Direction dir = RenderContext.Current.Direction;
                Font font = theme.GetFont(FontRole.Body);
                int size = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
                GUIStyle style = GuiStyleCache.Get(font, size);
                style.alignment = dir == Direction.Rtl ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
                style.wordWrap = true;
                Color saved = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextMuted);
                GUI.Label(RectSnap.Snap(rect), text, style);
                GUI.color = saved;
            };
            return node;
        }

        public static LightweaveNode Content(
            params LightweaveNode[] children
        ) {
            return ContentInternal(children);
        }

        private static LightweaveNode ContentInternal(
            LightweaveNode[] children,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            LightweaveNode node = NodeBuilder.New("Card.Content", line, file);
            for (int i = 0; i < children.Length; i++) {
                node.Children.Add(children[i]);
            }

            float contentGapPx = new Rem(0.5f).ToPixels();

            bool CanMeasureAll() {
                for (int i = 0; i < children.Length; i++) {
                    if (children[i].Measure == null && !children[i].PreferredHeight.HasValue) {
                        return false;
                    }
                }

                return children.Length > 0;
            }

            if (CanMeasureAll()) {
                node.Measure = availableWidth => {
                    float total = 0f;
                    for (int i = 0; i < children.Length; i++) {
                        LightweaveNode child = children[i];
                        total += child.Measure?.Invoke(availableWidth) ?? child.PreferredHeight ?? 0f;
                    }

                    total += contentGapPx * Mathf.Max(0, children.Length - 1);
                    return total;
                };
            }

            node.Paint = (rect, paintChildren) => {
                int count = children.Length;
                if (count == 0) {
                    return;
                }

                float y = rect.y;
                float totalGap = contentGapPx * Mathf.Max(0, count - 1);

                float[] resolvedHeights = new float[count];
                bool[] isFlex = new bool[count];
                float fixedTotal = 0f;
                int flexCount = 0;
                for (int i = 0; i < count; i++) {
                    LightweaveNode child = children[i];
                    float? h = child.Measure?.Invoke(rect.width) ?? child.PreferredHeight;
                    if (h.HasValue) {
                        resolvedHeights[i] = h.Value;
                        fixedTotal += h.Value;
                    } else {
                        isFlex[i] = true;
                        flexCount++;
                    }
                }

                float remainingForFlex = Mathf.Max(0f, rect.height - fixedTotal - totalGap);
                float flexEach = flexCount > 0 ? remainingForFlex / flexCount : 0f;

                for (int i = 0; i < count; i++) {
                    LightweaveNode child = children[i];
                    float h = isFlex[i] ? flexEach : resolvedHeights[i];
                    child.MeasuredRect = new Rect(rect.x, y, rect.width, h);
                    y += h + contentGapPx;
                }

                paintChildren();
            };
            return node;
        }

        public static LightweaveNode Footer(
            params LightweaveNode[] children
        ) {
            return FooterInternal(children);
        }

        private static LightweaveNode FooterInternal(
            LightweaveNode[] children,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            LightweaveNode node = NodeBuilder.New("Card.Footer", line, file);
            for (int i = 0; i < children.Length; i++) {
                node.Children.Add(children[i]);
            }

            node.PreferredHeight = new Rem(2.25f).ToPixels();

            node.Paint = (rect, paintChildren) => {
                int count = children.Length;
                if (count == 0) {
                    return;
                }

                Direction dir = RenderContext.Current.Direction;
                bool rtl = dir == Direction.Rtl;
                float gapPx = new Rem(0.5f).ToPixels();
                float totalGap = gapPx * Mathf.Max(0, count - 1);

                float totalWidth = 0f;
                float[] widths = new float[count];
                for (int i = 0; i < count; i++) {
                    float w = children[i].MeasuredRect.width > 0f ? children[i].MeasuredRect.width : new Rem(5f).ToPixels();
                    widths[i] = w;
                    totalWidth += w;
                }

                totalWidth += totalGap;

                float x = rtl ? rect.x : rect.xMax - totalWidth;
                for (int i = 0; i < count; i++) {
                    int idx = rtl ? count - 1 - i : i;
                    LightweaveNode child = children[idx];
                    child.MeasuredRect = new Rect(x, rect.y, widths[idx], rect.height);
                    x += widths[idx] + gapPx;
                }

                paintChildren();
            };
            return node;
        }
    }
}
