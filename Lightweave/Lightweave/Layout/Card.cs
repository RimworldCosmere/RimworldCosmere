using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Feedback;
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
        float[] fillMinHeights = new float[count];
        bool[] isFill = new bool[count];
        bool[] isLegacyFlex = new bool[count];
        int nonPinnedCount = pinnedLastIndex >= 0 ? count - 1 : count;
        float fixedTotalNonPinned = 0f;
        float fillMinTotal = 0f;
        int fillCount = 0;
        int legacyFlexCount = 0;

        for (int i = 0; i < count; i++) {
            LightweaveNode child = children[i];
            float? h = child.Measure?.Invoke(innerWidth) ?? child.PreferredHeight;
            if (i == pinnedLastIndex) {
                heights[i] = h ?? new Rem(2.25f).ToPixels();
                continue;
            }

            if (child.IsCardContent) {
                isFill[i] = true;
                fillMinHeights[i] = h ?? 0f;
                fillMinTotal += fillMinHeights[i];
                fillCount++;
            }
            else if (h.HasValue) {
                heights[i] = h.Value;
                fixedTotalNonPinned += h.Value;
            }
            else {
                isLegacyFlex[i] = true;
                legacyFlexCount++;
            }
        }

        float pinnedH = pinnedLastIndex >= 0 ? heights[pinnedLastIndex] : 0f;
        float pinnedGap = pinnedLastIndex >= 0 && nonPinnedCount > 0 ? gapPx : 0f;
        float availableForNonPinned = Mathf.Max(0f, availableHeight - pinnedH - pinnedGap);
        float nonPinnedTotalGap = gapPx * Mathf.Max(0, nonPinnedCount - 1);
        float remainingAfterFixed = Mathf.Max(0f, availableForNonPinned - fixedTotalNonPinned - nonPinnedTotalGap);

        if (fillCount > 0) {
            float extra = Mathf.Max(0f, remainingAfterFixed - fillMinTotal);
            float fillExtraEach = extra / fillCount;
            for (int i = 0; i < count; i++) {
                if (isFill[i]) {
                    heights[i] = fillMinHeights[i] + fillExtraEach;
                }
            }
        }
        else if (legacyFlexCount > 0) {
            float flexEach = remainingAfterFixed / legacyFlexCount;
            for (int i = 0; i < count; i++) {
                if (isLegacyFlex[i]) {
                    heights[i] = flexEach;
                }
            }
        }

        return heights;
    }

    public static LightweaveNode Create(
        Action<List<LightweaveNode>>? children = null,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        return CreateInternal(kids.ToArray(), style, classes, id, line, file);
    }

    private static LightweaveNode CreateInternal(
        LightweaveNode[] children,
        Style? style,
        string[]? classes,
        string? id,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        BackgroundSpec defaultBg = BackgroundSpec.Of(ThemeSlot.SurfaceRaised);
        BorderSpec defaultBorder = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
        RadiusSpec defaultRadius = RadiusSpec.All(RadiusScale.Lg);

        LightweaveNode node = NodeBuilder.New("Card", line, file);
        node.ApplyStyling("card", style, classes, id);

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
            node.Measure = availableWidth => {
                Style s = node.GetResolvedStyle();
                EdgeInsets pad = s.Padding ?? EdgeInsets.Zero;
                float padTopPx = pad.Top?.ToPixels() ?? 0f;
                float padBottomPx = pad.Bottom?.ToPixels() ?? 0f;
                Direction renderDir = RenderContext.Current.Direction;
                Rect dummy = new Rect(0f, 0f, availableWidth, 0f);
                Rect content = pad.Shrink(dummy, renderDir);
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
            Style s = node.GetResolvedStyle();
            EdgeInsets pad = s.Padding ?? EdgeInsets.Zero;
            BackgroundSpec bg = s.Background ?? defaultBg;
            BorderSpec border = s.Border ?? defaultBorder;
            RadiusSpec radius = s.Radius ?? defaultRadius;

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

    public static LightweaveNode Header(
        Action<List<LightweaveNode>>? children = null,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        return HeaderInternal(kids.ToArray(), style, classes, id, line, file);
    }

    private static LightweaveNode HeaderInternal(
        LightweaveNode[] children,
        Style? style,
        string[]? classes,
        string? id,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Card.Header", line, file);
        node.ApplyStyling("card-header", style, classes, id);
        for (int i = 0; i < children.Length; i++) {
            node.Children.Add(children[i]);
        }

        float gapPx = new Rem(0.25f).ToPixels();
        float fallbackH = new Rem(1.5f).ToPixels();
        EdgeInsets defaultPad = EdgeInsets.All(SpacingScale.Md);

        node.Measure = availableWidth => {
            Style s = node.GetResolvedStyle();
            EdgeInsets pad = s.Padding ?? defaultPad;
            float padTopPx = pad.Top?.ToPixels() ?? 0f;
            float padBottomPx = pad.Bottom?.ToPixels() ?? 0f;
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
            Style s = node.GetResolvedStyle();
            EdgeInsets pad = s.Padding ?? defaultPad;
            BorderSpec border = s.Border ?? divider;

            PaintBox.Draw(rect, s.Background, border, null);

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

    public static LightweaveNode Title(
        string text,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Card.Title:{text}", line, file);
        node.ApplyStyling("card-title", style, classes, id);
        node.PreferredHeight = new Rem(1.5f).ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            Font font = theme.GetFont(FontRole.BodyBold);
            int size = Mathf.RoundToInt(new Rem(1.125f).ToFontPx());
            GUIStyle gstyle = GuiStyleCache.GetOrCreate(font, size, FontStyle.Bold);
            gstyle.alignment = dir == Direction.Rtl ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
            GUI.Label(RectSnap.Snap(rect), text, gstyle);
            GUI.color = saved;
        };
        return node;
    }

    public static LightweaveNode Description(
        string text,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Card.Description:{text}", line, file);
        node.ApplyStyling("card-description", style, classes, id);
        node.PreferredHeight = new Rem(1.25f).ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            Font font = theme.GetFont(FontRole.Body);
            int size = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
            GUIStyle gstyle = GuiStyleCache.GetOrCreate(font, size);
            gstyle.alignment = dir == Direction.Rtl ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            gstyle.wordWrap = true;
            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            GUI.Label(RectSnap.Snap(rect), text, gstyle);
            GUI.color = saved;
        };
        return node;
    }

    public static LightweaveNode Content(
        Action<List<LightweaveNode>>? children = null,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        return ContentInternal(kids.ToArray(), style, classes, id, line, file);
    }

private static LightweaveNode ContentInternal(
        LightweaveNode[] children,
        Style? style,
        string[]? classes,
        string? id,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Card.Content", line, file);
        node.ApplyStyling("card-content", style, classes, id);
        node.IsCardContent = true;
        for (int i = 0; i < children.Length; i++) {
            node.Children.Add(children[i]);
        }

        float contentGapPx = new Rem(0.5f).ToPixels();
        EdgeInsets defaultPad = EdgeInsets.All(SpacingScale.Md);
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

        if (CanMeasureAll()) {
            node.Measure = availableWidth => {
                Style s = node.GetResolvedStyle();
                EdgeInsets pad = s.Padding ?? defaultPad;
                float padTopPx = pad.Top?.ToPixels() ?? 0f;
                float padBottomPx = pad.Bottom?.ToPixels() ?? 0f;
                Direction renderDir = RenderContext.Current.Direction;
                Rect dummy = new Rect(0f, 0f, availableWidth, 0f);
                float innerWidth = pad.Shrink(dummy, renderDir).width;
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
            Style s = node.GetResolvedStyle();
            EdgeInsets pad = s.Padding ?? defaultPad;
            BorderSpec border = s.Border ?? sideBorder;
            if (s.Background != null) {
                PaintBox.Draw(rect, s.Background, null, null);
            }

            PaintBox.Draw(rect, null, border, null);

            Direction renderDir = RenderContext.Current.Direction;
            Rect inner = pad.Shrink(rect, renderDir);

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

    public static LightweaveNode Footer(
        Action<List<LightweaveNode>>? children = null,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        return FooterInternal(kids.ToArray(), style, classes, id, line, file);
    }

    private static LightweaveNode FooterInternal(
        LightweaveNode[] children,
        Style? style,
        string[]? classes,
        string? id,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Card.Footer", line, file);
        node.ApplyStyling("card-footer", style, classes, id);
        node.IsFooter = true;
        for (int i = 0; i < children.Length; i++) {
            node.Children.Add(children[i]);
        }

        EdgeInsets defaultPad = EdgeInsets.All(SpacingScale.Md);
        float buttonRowH = new Rem(2.25f).ToPixels();
        float gapPx = new Rem(0.5f).ToPixels();

        Style s0 = node.GetResolvedStyle();
        EdgeInsets pad0 = s0.Padding ?? defaultPad;
        float padTopPx0 = pad0.Top?.ToPixels() ?? 0f;
        float padBottomPx0 = pad0.Bottom?.ToPixels() ?? 0f;
        node.PreferredHeight = buttonRowH + padTopPx0 + padBottomPx0;

        BorderSpec divider = new BorderSpec(Top: new Rem(1f / 16f), Color: ThemeSlot.BorderDefault);

        node.Paint = (rect, paintChildren) => {
            Style s = node.GetResolvedStyle();
            EdgeInsets pad = s.Padding ?? defaultPad;
            BorderSpec border = s.Border ?? divider;

            PaintBox.Draw(rect, s.Background, border, null);

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

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        LightweaveNode StatRow(string label, string value) {
            return HStack.Create(
                SpacingScale.Sm,
                children: r => {
                    r.AddFlex(Caption.Create(label));
                    r.AddHug(Text.Create(
                        value,
                        style: new Style { FontFamily = FontRole.BodyBold, FontSize = new Rem(0.875f), TextColor = ThemeSlot.TextPrimary }
                    ));
                }
            );
        }

        return new DocSample(() => 
            Card.Create(c => {
                c.Add(Card.Header(h => {
                    h.Add(Card.Title((string)"CL_Playground_card_Pawn_Name".Translate()));
                    h.Add(Card.Description((string)"CL_Playground_card_Pawn_Role".Translate()));
                }));
                c.Add(Card.Content(ct => {
                    ct.Add(StatRow((string)"CL_Playground_card_Pawn_StatHealth".Translate(), (string)"CL_Playground_card_Pawn_StatHealthValue".Translate()));
                    ct.Add(StatRow((string)"CL_Playground_card_Pawn_StatMood".Translate(), (string)"CL_Playground_card_Pawn_StatMoodValue".Translate()));
                    ct.Add(StatRow((string)"CL_Playground_card_Pawn_StatLight".Translate(), (string)"CL_Playground_card_Pawn_StatLightValue".Translate()));
                    ct.Add(Divider.Horizontal());
                    ct.Add(Text.Create(
                        (string)"CL_Playground_card_Pawn_Note".Translate(),
                        wrap: true,
                        style: new Style { FontFamily = FontRole.Body, FontSize = new Rem(0.8125f), TextColor = ThemeSlot.TextMuted }
                    ));
                }));
                c.Add(Card.Footer(f => {
                    f.Add(Button.Create((string)"CL_Playground_card_Pawn_Action_Dismiss".Translate(), () => { }, ButtonVariant.Secondary));
                    f.Add(Button.Create((string)"CL_Playground_card_Pawn_Action_Promote".Translate(), () => { }, ButtonVariant.Primary));
                }));
            })
        );
    }

    [DocVariant("CL_Playground_Label_Tight", Order = 1)]
    public static DocSample DocsTight() {
        return new DocSample(() => 
            Card.Create(c => {
                c.Add(Card.Header(h => {
                    h.Add(HStack.Create(
                        SpacingScale.Xs,
                        children: r => {
                            r.AddFlex(Card.Title((string)"CL_Playground_card_Tight_Title".Translate()));
                            r.AddHug(Pill.Create(
                                (string)"CL_Playground_card_Tight_Pill".Translate(),
                                variant: PillVariant.Selected
                            ));
                        }
                    ));
                }));
                c.Add(Card.Content(ct => {
                    ct.Add(Text.Create(
                        (string)"CL_Playground_card_Tight_Note".Translate(),
                        wrap: true,
                        style: new Style { FontFamily = FontRole.Body, FontSize = new Rem(0.8125f), TextColor = ThemeSlot.TextMuted }
                    ));
                }));
            })
        );
    }

    [DocVariant("CL_Playground_Label_Loose", Order = 2)]
    public static DocSample DocsLoose() {
        return new DocSample(() => 
            Card.Create(c => {
                c.Add(Card.Header(h => {
                    h.Add(Card.Title((string)"CL_Playground_card_Loose_Title".Translate()));
                    h.Add(Card.Description((string)"CL_Playground_card_Loose_Desc".Translate()));
                }));
                c.Add(Card.Footer(f => {
                    f.Add(Button.Create((string)"CL_Playground_card_Loose_Cancel".Translate(), () => { }, ButtonVariant.Ghost));
                    f.Add(Button.Create((string)"CL_Playground_card_Loose_Confirm".Translate(), () => { }, ButtonVariant.Primary));
                }));
            })
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => 
            Card.Create(c => {
                c.Add(Card.Header(h => {
                    h.Add(Card.Title("Surgebinding"));
                    h.Add(Card.Description("Bonded Radiant powers."));
                }));
                c.Add(Card.Content(ct => ct.Add(Text.Create("Progression unlocks with oaths."))));
                c.Add(Card.Footer(f => f.Add(Button.Create("Confirm", () => { }))));
            })
        );
    }
}
