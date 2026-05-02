using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Typography.Typography;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Layout;

public static partial class Layout {
    [Doc(
        Id = "card",
        Summary = "Filled rectangular container with optional header/footer.",
        WhenToUse = "Group related controls under one frame.",
        SourcePath = "Lightweave/Lightweave/Layout/Card.cs",
        PreferredVariantHeight = 280f
    )]
    public static class Card {
        private static float[] ResolveChildHeights(
            LightweaveNode[] children,
            float innerWidth,
            float availableHeight,
            float gapPx,
            int pinnedLastIndex
        ) {
            int count = children.Length;
            float[] heights = new float[count];
            bool[] isFlex = new bool[count];
            int nonPinnedCount = pinnedLastIndex >= 0 ? count - 1 : count;
            float fixedTotalNonPinned = 0f;
            int flexCount = 0;

            for (int i = 0; i < count; i++) {
                LightweaveNode child = children[i];
                float? h = child.Measure?.Invoke(innerWidth) ?? child.PreferredHeight;
                if (i == pinnedLastIndex) {
                    heights[i] = h ?? new Rem(2.25f).ToPixels();
                    continue;
                }

                if (h.HasValue) {
                    heights[i] = h.Value;
                    fixedTotalNonPinned += h.Value;
                } else {
                    isFlex[i] = true;
                    flexCount++;
                }
            }

            float pinnedH = pinnedLastIndex >= 0 ? heights[pinnedLastIndex] : 0f;
            float pinnedGap = pinnedLastIndex >= 0 && nonPinnedCount > 0 ? gapPx : 0f;
            float availableForNonPinned = Mathf.Max(0f, availableHeight - pinnedH - pinnedGap);
            float nonPinnedTotalGap = gapPx * Mathf.Max(0, nonPinnedCount - 1);
            float remainingForFlex = Mathf.Max(0f, availableForNonPinned - fixedTotalNonPinned - nonPinnedTotalGap);
            float flexEach = flexCount > 0 ? remainingForFlex / flexCount : 0f;

            for (int i = 0; i < count; i++) {
                if (i == pinnedLastIndex) {
                    continue;
                }

                if (isFlex[i]) {
                    heights[i] = flexEach;
                }
            }

            return heights;
        }

        public static LightweaveNode Create(
            [DocParam("Section nodes composed into the card (Header, Content, Footer, or arbitrary nodes).")]
            params LightweaveNode[] children
        ) {
            return CreateInternal(null, children);
        }

        public static LightweaveNode WithPadding(
            [DocParam("Inner padding applied uniformly. Overrides the default SpacingScale.Md.", TypeOverride = "Rem", DefaultOverride = "SpacingScale.Md")]
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
            EdgeInsets pad = padding ?? EdgeInsets.Zero;
            BackgroundSpec bg = new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised);
            BorderSpec border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
            RadiusSpec radius = RadiusSpec.All(new Rem(0.5f));

            LightweaveNode node = NodeBuilder.New("Card", line, file);
            for (int i = 0; i < children.Length; i++) {
                node.Children.Add(children[i]);
            }

            float gapPxStatic = 0f;
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

                float gapPx = 0f;

                int footerIdx = -1;
                for (int i = count - 1; i >= 0; i--) {
                    if (children[i].IsFooter) {
                        footerIdx = i;
                        break;
                    }
                }

                float[] resolvedHeights = ResolveChildHeights(children, content.width, content.height, gapPx, footerIdx);

                float y = content.y;
                for (int i = 0; i < count; i++) {
                    if (i == footerIdx) {
                        continue;
                    }

                    LightweaveNode child = children[i];
                    child.MeasuredRect = new Rect(content.x, y, content.width, resolvedHeights[i]);
                    y += resolvedHeights[i] + gapPx;
                }

                if (footerIdx >= 0) {
                    float footerH = resolvedHeights[footerIdx];
                    children[footerIdx].MeasuredRect = new Rect(content.x, content.yMax - footerH, content.width, footerH);
                }

                paintChildren();
            };
            return node;
        }

        [Doc(Slot = true, Summary = "Title row of a Card.")]
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
            EdgeInsets pad = EdgeInsets.All(SpacingScale.Md);
            float padTopPx = pad.Top?.ToPixels() ?? 0f;
            float padBottomPx = pad.Bottom?.ToPixels() ?? 0f;

            node.Measure = availableWidth => {
                Direction renderDir = RenderContext.Current.Direction;
                Rect dummy = new Rect(0f, 0f, availableWidth, 0f);
                float innerWidth = pad.Shrink(dummy, renderDir).width;
                float total = 0f;
                for (int i = 0; i < children.Length; i++) {
                    LightweaveNode child = children[i];
                    total += child.Measure?.Invoke(innerWidth) ?? child.PreferredHeight ?? fallbackH;
                }

                if (children.Length > 1) {
                    total += gapPx * (children.Length - 1);
                }

                total += padTopPx + padBottomPx;
                return total;
            };

            BorderSpec divider = new BorderSpec(Bottom: new Rem(1f / 16f), Color: ThemeSlot.BorderDefault);

            node.Paint = (rect, paintChildren) => {
                PaintBox.Draw(rect, null, divider, null);

                Rect inner = pad.Shrink(rect, RenderContext.Current.Direction);
                float y = inner.y;
                for (int i = 0; i < children.Length; i++) {
                    LightweaveNode child = children[i];
                    float h = child.Measure?.Invoke(inner.width) ?? child.PreferredHeight ?? fallbackH;
                    child.MeasuredRect = new Rect(inner.x, y, inner.width, h);
                    y += h + gapPx;
                }

                paintChildren();
            };
            return node;
        }

        [Doc(Slot = true, Summary = "Heading text inside a Header.", ParentSlot = nameof(Header))]
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

        [Doc(Slot = true, Summary = "Subtitle text inside a Header.", ParentSlot = nameof(Header))]
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

        [Doc(Slot = true, Summary = "Body region of a Card.")]
        public static LightweaveNode Content(
            params LightweaveNode[] children
        ) {
            return ContentInternal(children, null);
        }

        public static LightweaveNode Content(
            ThemeSlot background,
            params LightweaveNode[] children
        ) {
            return ContentInternal(children, new BackgroundSpec.Solid(background));
        }

        private static LightweaveNode ContentInternal(
            LightweaveNode[] children,
            BackgroundSpec? background,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            LightweaveNode node = NodeBuilder.New("Card.Content", line, file);
            for (int i = 0; i < children.Length; i++) {
                node.Children.Add(children[i]);
            }

            float contentGapPx = new Rem(0.5f).ToPixels();
            EdgeInsets innerPad = EdgeInsets.All(SpacingScale.Md);
            float padTopPx = innerPad.Top?.ToPixels() ?? 0f;
            float padBottomPx = innerPad.Bottom?.ToPixels() ?? 0f;
            BorderSpec sideBorder = new BorderSpec(
                Left: new Rem(1f / 16f),
                Right: new Rem(1f / 16f),
                Color: ThemeSlot.BorderDefault
            );

            bool CanMeasureAll() {
                for (int i = 0; i < children.Length; i++) {
                    if (children[i].Measure == null && !children[i].PreferredHeight.HasValue) {
                        return false;
                    }
                }

                return children.Length > 0;
            }

            if (background == null && CanMeasureAll()) {
                node.Measure = availableWidth => {
                    Direction renderDir = RenderContext.Current.Direction;
                    Rect dummy = new Rect(0f, 0f, availableWidth, 0f);
                    float innerWidth = innerPad.Shrink(dummy, renderDir).width;
                    float total = 0f;
                    for (int i = 0; i < children.Length; i++) {
                        LightweaveNode child = children[i];
                        total += child.Measure?.Invoke(innerWidth) ?? child.PreferredHeight ?? 0f;
                    }

                    total += contentGapPx * Mathf.Max(0, children.Length - 1);
                    total += padTopPx + padBottomPx;
                    return total;
                };
            }

            node.Paint = (rect, paintChildren) => {
                if (background != null) {
                    PaintBox.Draw(rect, background, null, null);
                }

                PaintBox.Draw(rect, null, sideBorder, null);

                Direction renderDir = RenderContext.Current.Direction;
                Rect inner = innerPad.Shrink(rect, renderDir);

                int count = children.Length;
                if (count == 0) {
                    return;
                }

                float[] resolvedHeights = ResolveChildHeights(children, inner.width, inner.height, contentGapPx, -1);

                float y = inner.y;
                for (int i = 0; i < count; i++) {
                    LightweaveNode child = children[i];
                    child.MeasuredRect = new Rect(inner.x, y, inner.width, resolvedHeights[i]);
                    y += resolvedHeights[i] + contentGapPx;
                }

                paintChildren();
            };
            return node;
        }

        [Doc(Slot = true, Summary = "Action row at the bottom of a Card.")]
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
            node.IsFooter = true;
            for (int i = 0; i < children.Length; i++) {
                node.Children.Add(children[i]);
            }

            EdgeInsets pad = EdgeInsets.All(SpacingScale.Md);
            float padTopPx = pad.Top?.ToPixels() ?? 0f;
            float padBottomPx = pad.Bottom?.ToPixels() ?? 0f;
            float buttonRowH = new Rem(2.25f).ToPixels();
            float gapPx = new Rem(0.5f).ToPixels();
            node.PreferredHeight = buttonRowH + padTopPx + padBottomPx;

            BorderSpec divider = new BorderSpec(Top: new Rem(1f / 16f), Color: ThemeSlot.BorderDefault);

            node.Paint = (rect, paintChildren) => {
                PaintBox.Draw(rect, null, divider, null);

                int count = children.Length;
                if (count == 0) {
                    return;
                }

                Direction dir = RenderContext.Current.Direction;
                bool rtl = dir == Direction.Rtl;
                Rect inner = pad.Shrink(rect, dir);
                float totalGap = gapPx * Mathf.Max(0, count - 1);

                float totalWidth = 0f;
                float[] widths = new float[count];
                for (int i = 0; i < count; i++) {
                    float w = children[i].MeasuredRect.width > 0f ? children[i].MeasuredRect.width : new Rem(5f).ToPixels();
                    widths[i] = w;
                    totalWidth += w;
                }

                totalWidth += totalGap;

                float x = rtl ? inner.x : inner.xMax - totalWidth;
                for (int i = 0; i < count; i++) {
                    int idx = rtl ? count - 1 - i : i;
                    LightweaveNode child = children[idx];
                    child.MeasuredRect = new Rect(x, inner.y, widths[idx], inner.height);
                    x += widths[idx] + gapPx;
                }

                paintChildren();
            };
            return node;
        }

        [DocVariant("CC_Playground_Label_Default")]
        public static DocSample DocsDefault() {
            return new DocSample(
                Card.Create(
                    Card.Header(
                        Card.Title("Surgebinding"),
                        Card.Description("Bonded Radiant powers.")
                    ),
                    Card.Content(
                        ThemeSlot.SurfaceSunken,
                        Text.Create(
                            "Progression unlocks with oaths.",
                            FontRole.Body,
                            new Rem(0.875f),
                            ThemeSlot.TextPrimary
                        )
                    ),
                    Card.Footer(
                        Button.Create((string)"CC_Playground_Label_Cancel".Translate(), () => { }),
                        Button.Create((string)"CC_Playground_Label_Confirm".Translate(), () => { })
                    )
                )
            );
        }

        [DocVariant("CC_Playground_Label_Tight", Order = 1)]
        public static DocSample DocsTight() {
            return new DocSample(
                Card.Create(
                    Card.Header(
                        Card.Title("Compact")
                    ),
                    Card.Content(
                        Text.Create(
                            "Minimal layout for a short note.",
                            FontRole.Body,
                            new Rem(0.875f),
                            ThemeSlot.TextMuted
                        )
                    )
                )
            );
        }

        [DocVariant("CC_Playground_Label_Loose", Order = 2)]
        public static DocSample DocsLoose() {
            return new DocSample(
                Card.Create(
                    Card.Header(
                        Card.Title("Confirm action"),
                        Card.Description("Generous content suits modal flows.")
                    ),
                    Card.Footer(
                        Button.Create((string)"CC_Playground_Label_Cancel".Translate(), () => { }),
                        Button.Create((string)"CC_Playground_Label_Confirm".Translate(), () => { })
                    )
                )
            );
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(
                Card.Create(
                    Card.Header(
                        Card.Title("Surgebinding"),
                        Card.Description("Bonded Radiant powers.")
                    ),
                    Card.Content(
                        Text.Create("Progression unlocks with oaths.")
                    ),
                    Card.Footer(
                        Button.Create("Confirm", () => { })
                    )
                )
            );
        }
    }
}
