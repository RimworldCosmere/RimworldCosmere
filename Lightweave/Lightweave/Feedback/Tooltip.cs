using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Feedback;

public enum TooltipSide {
    Top,
    TopStart,
    TopEnd,
    Right,
    RightStart,
    RightEnd,
    Bottom,
    BottomStart,
    BottomEnd,
    Left,
    LeftStart,
    LeftEnd,
}

public enum TooltipAlign {
    Start,
    Center,
    End,
}

[Doc(
    Id = "tooltip",
    Summary = "Hover-delayed contextual hint anchored to a trigger element.",
    WhenToUse = "Reveal short clarifying text on hover without claiming layout space.",
    SourcePath = "Lightweave/Lightweave/Feedback/Tooltip.cs"
)]
public static class Tooltip {
    private const float DefaultDelaySeconds = 0.5f;
    private const float DefaultSideOffsetPx = 4f;

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => Tooltip.Create(
            Button.Create("Hover me", () => { }, ButtonVariant.Secondary),
            "A single-line tooltip."
        ));
    }

    [DocVariant("CL_Playground_Tooltip_Variant_AllSides")]
    public static DocSample DocsAllSides() {
        return new DocSample(() => Stack.Create(
            gap: SpacingScale.Xxl,
            children: outer => {
                outer.Add(HStack.Create(
                    gap: SpacingScale.Lg,
                    children: row => {
                        row.AddFlex(Tooltip.Create(
                            Button.Create("TopStart", () => { }, ButtonVariant.Secondary),
                            "Anchored top, start-aligned.",
                            side: TooltipSide.TopStart
                        ));
                        row.AddFlex(Tooltip.Create(
                            Button.Create("Top", () => { }, ButtonVariant.Secondary),
                            "Anchored top, centered.",
                            side: TooltipSide.Top
                        ));
                        row.AddFlex(Tooltip.Create(
                            Button.Create("TopEnd", () => { }, ButtonVariant.Secondary),
                            "Anchored top, end-aligned.",
                            side: TooltipSide.TopEnd
                        ));
                    }
                ));
                outer.Add(HStack.Create(
                    gap: SpacingScale.Lg,
                    children: row => {
                        row.AddFlex(Tooltip.Create(
                            Button.Create("Left", () => { }, ButtonVariant.Secondary),
                            "Anchored to the left.",
                            side: TooltipSide.Left
                        ));
                        row.AddFlex(Box.Create());
                        row.AddFlex(Tooltip.Create(
                            Button.Create("Right", () => { }, ButtonVariant.Secondary),
                            "Anchored to the right.",
                            side: TooltipSide.Right
                        ));
                    }
                ));
                outer.Add(HStack.Create(
                    gap: SpacingScale.Lg,
                    children: row => {
                        row.AddFlex(Tooltip.Create(
                            Button.Create("BottomStart", () => { }, ButtonVariant.Secondary),
                            "Anchored bottom, start-aligned.",
                            side: TooltipSide.BottomStart
                        ));
                        row.AddFlex(Tooltip.Create(
                            Button.Create("Bottom", () => { }, ButtonVariant.Secondary),
                            "Anchored bottom, centered.",
                            side: TooltipSide.Bottom
                        ));
                        row.AddFlex(Tooltip.Create(
                            Button.Create("BottomEnd", () => { }, ButtonVariant.Secondary),
                            "Anchored bottom, end-aligned.",
                            side: TooltipSide.BottomEnd
                        ));
                    }
                ));
            }
        ));
    }

    [DocVariant("CL_Playground_Tooltip_Variant_LongDelay")]
    public static DocSample DocsLongDelay() {
        return new DocSample(() => Tooltip.Create(
            Button.Create("Patient", () => { }, ButtonVariant.Secondary),
            "Two-second delay before this appears.",
            delayDuration: 2f
        ));
    }

    [DocVariant("CL_Playground_Tooltip_Variant_NoDelay")]
    public static DocSample DocsNoDelay() {
        return new DocSample(() => Tooltip.Create(
            Button.Create("Instant", () => { }, ButtonVariant.Secondary),
            "Appears immediately on hover.",
            delayDuration: 0f
        ));
    }

    [DocVariant("CL_Playground_Tooltip_Variant_LargeOffset")]
    public static DocSample DocsLargeOffset() {
        return new DocSample(() => Tooltip.Create(
            Button.Create("Far", () => { }, ButtonVariant.Secondary),
            "20px offset from the trigger.",
            sideOffset: 20f
        ));
    }

    [DocVariant("CL_Playground_Tooltip_Variant_Wrapping")]
    public static DocSample DocsWrapping() {
        return new DocSample(() => Tooltip.Create(
            Button.Create("Long body", () => { }, ButtonVariant.Secondary),
            "This tooltip body wraps onto multiple lines because it exceeds the maximum width that the maxWidth parameter constrains it to.",
            maxWidth: new Rem(12f)
        ));
    }

    [DocVariant("CL_Playground_Tooltip_Variant_Disabled")]
    public static DocSample DocsOnDisabled() {
        return new DocSample(() => Tooltip.Create(
            Button.Create("Disabled", () => { }, ButtonVariant.Secondary, disabled: true),
            "Disabled triggers still surface their tooltip on hover."
        ));
    }

    [DocVariant("CL_Playground_Tooltip_Variant_Live")]
    public static DocSample DocsLive() {
        return new DocSample(() => {
            Hooks.Hooks.StateHandle<int> ticks = Hooks.Hooks.UseState(0);
            return Tooltip.Create(
                Button.Create("Tick", () => ticks.Set(ticks.Value + 1), ButtonVariant.Secondary),
                () => $"Clicked {ticks.Value} time(s).",
                side: TooltipSide.Bottom
            );
        });
    }

    [DocVariant("CL_Playground_Tooltip_Variant_RichContent")]
    public static DocSample DocsRichContent() {
        return new DocSample(() => Tooltip.Create(
            Button.Create("Rich", () => { }, ButtonVariant.Secondary),
            BuildRichBody(),
            new Vector2(new Rem(14f).ToPixels(), new Rem(4.5f).ToPixels())
        ));
    }

    private static LightweaveNode BuildRichBody() {
        return Stack.Create(
            gap: SpacingScale.Xs,
            children: b => {
                b.Add(Typography.Typography.Heading.Create(4, "Stormlight"));
                b.Add(Typography.Typography.Text.Create("412 / 1000", FontRole.Body, new Rem(0.875f), ThemeSlot.TextSecondary));
            }
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => Tooltip.Create(
            Button.Create("Hover me", () => { }, ButtonVariant.Secondary),
            "Hint shown after a brief hover."
        ));
    }

    public static LightweaveNode Create(
        [DocParam("Element the tooltip is anchored to. Receives all layout space; tooltip overlays separately.")]
        LightweaveNode children,
        [DocParam("Static hint text. Wrapped automatically up to maxWidth.")]
        string text,
        [DocParam("Anchor side and optional Start/End suffix. Suffix overrides align.")]
        TooltipSide side = TooltipSide.Bottom,
        [DocParam("Cross-axis alignment when side is cardinal (Top/Right/Bottom/Left).")]
        TooltipAlign align = TooltipAlign.Center,
        [DocParam("Hover seconds before the tooltip appears. 0 = instant.")]
        float delayDuration = DefaultDelaySeconds,
        [DocParam("Pixel gap between trigger and tooltip on the anchor axis.")]
        float sideOffset = DefaultSideOffsetPx,
        [DocParam("Maximum content width before wrapping. Default 20rem.")]
        Rem? maxWidth = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Vector2 size = MeasureText(text, maxWidth);
        return CreateInternal(
            children,
            () => BuildTextNode(text),
            () => size,
            side,
            align,
            delayDuration,
            sideOffset,
            line,
            file
        );
    }

    public static LightweaveNode Create(
        [DocParam("Element the tooltip is anchored to.")]
        LightweaveNode children,
        [DocParam("Delegate returning hint text. Re-evaluated each frame the tooltip is visible.")]
        Func<string> text,
        [DocParam("Anchor side. Suffix overrides align.")]
        TooltipSide side = TooltipSide.Bottom,
        [DocParam("Cross-axis alignment for cardinal sides.")]
        TooltipAlign align = TooltipAlign.Center,
        [DocParam("Hover seconds before the tooltip appears.")]
        float delayDuration = DefaultDelaySeconds,
        [DocParam("Pixel gap between trigger and tooltip.")]
        float sideOffset = DefaultSideOffsetPx,
        [DocParam("Maximum content width before wrapping.")]
        Rem? maxWidth = null,
        [DocParam("Optional dynamic anchor rect for positioning. When set, hover still uses the children rect, but the tooltip is placed relative to this rect (e.g. a hovered point on a chart).")]
        Func<Rect>? anchor = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Func<string> resolver = text;
        Rem? widthCap = maxWidth;
        return CreateInternal(
            children,
            () => BuildTextNode(resolver()),
            () => MeasureText(resolver(), widthCap),
            side,
            align,
            delayDuration,
            sideOffset,
            line,
            file,
            anchor
        );
    }

    public static LightweaveNode Create(
        [DocParam("Element the tooltip is anchored to.")]
        LightweaveNode children,
        [DocParam("Custom node painted inside the tooltip surface.")]
        LightweaveNode content,
        [DocParam("Pixel size of the tooltip surface.")]
        Vector2 preferredSize,
        [DocParam("Anchor side. Suffix overrides align.")]
        TooltipSide side = TooltipSide.Bottom,
        [DocParam("Cross-axis alignment for cardinal sides.")]
        TooltipAlign align = TooltipAlign.Center,
        [DocParam("Hover seconds before the tooltip appears.")]
        float delayDuration = DefaultDelaySeconds,
        [DocParam("Pixel gap between trigger and tooltip.")]
        float sideOffset = DefaultSideOffsetPx,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        return CreateInternal(
            children,
            () => content,
            () => preferredSize,
            side,
            align,
            delayDuration,
            sideOffset,
            line,
            file
        );
    }

    public static LightweaveNode Create(
        [DocParam("Element the tooltip is anchored to.")]
        LightweaveNode children,
        [DocParam("Delegate building the tooltip body. Rebuilt each frame the tooltip is visible.")]
        Func<LightweaveNode> content,
        [DocParam("Delegate returning the tooltip surface size each frame.")]
        Func<Vector2> preferredSize,
        [DocParam("Anchor side. Suffix overrides align.")]
        TooltipSide side = TooltipSide.Bottom,
        [DocParam("Cross-axis alignment for cardinal sides.")]
        TooltipAlign align = TooltipAlign.Center,
        [DocParam("Hover seconds before the tooltip appears.")]
        float delayDuration = DefaultDelaySeconds,
        [DocParam("Pixel gap between trigger and tooltip.")]
        float sideOffset = DefaultSideOffsetPx,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        return CreateInternal(
            children,
            content,
            preferredSize,
            side,
            align,
            delayDuration,
            sideOffset,
            line,
            file
        );
    }

    private static LightweaveNode CreateInternal(
        LightweaveNode children,
        Func<LightweaveNode> contentFactory,
        Func<Vector2> sizeFactory,
        TooltipSide side,
        TooltipAlign align,
        float delayDuration,
        float sideOffset,
        int line,
        string file,
        Func<Rect>? anchorOverride = null
    ) {
        LightweaveNode node = NodeBuilder.New("Tooltip", line, file);
        node.Children.Add(children);

        node.Measure = availableWidth => children.Measure?.Invoke(availableWidth) ?? children.PreferredHeight ?? 0f;

        node.Paint = (rect, _) => {
            children.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(children, rect);

            Rect hoverRect = children.MeasuredRect;

            Hooks.Hooks.RefHandle<float> hoverTimer = Hooks.Hooks.UseRef(0f, line, file);

            bool hovered = Mouse.IsOver(hoverRect);
            Event e = Event.current;

            if (!hovered || e.type == EventType.MouseDown) {
                hoverTimer.Current = 0f;
                return;
            }

            if (e.type == EventType.Repaint) {
                hoverTimer.Current += Time.unscaledDeltaTime;
            }

            if (hoverTimer.Current < delayDuration) {
                return;
            }

            Rect anchorRect = anchorOverride?.Invoke() ?? hoverRect;
            Vector2 size = sizeFactory();
            Rect tooltipScreenRect = ResolveScreenRect(anchorRect, size, side, align, sideOffset);
            LightweaveNode content = contentFactory();

            RenderContext.Current.PendingOverlays.Enqueue(() => {
                Vector2 local = GUIUtility.ScreenToGUIPoint(new Vector2(tooltipScreenRect.x, tooltipScreenRect.y));
                Rect tooltipRect = new Rect(local.x, local.y, tooltipScreenRect.width, tooltipScreenRect.height);

                Rect shadowRect = new Rect(
                    tooltipRect.x + 2f,
                    tooltipRect.y + 3f,
                    tooltipRect.width,
                    tooltipRect.height
                );
                PaintBox.Draw(shadowRect, BackgroundSpec.Of(ThemeSlot.SurfaceShadow), null, null);

                PaintBox.Draw(
                    tooltipRect,
                    BackgroundSpec.Of(ThemeSlot.SurfaceRaised),
                    BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault),
                    RadiusSpec.All(new Rem(0.25f))
                );

                float pad = new Rem(0.5f).ToPixels();
                Rect innerRect = new Rect(
                    tooltipRect.x + pad,
                    tooltipRect.y + pad,
                    tooltipRect.width - pad * 2f,
                    tooltipRect.height - pad * 2f
                );

                LightweaveRoot.PaintSubtree(content, innerRect);
            });
        };

        return node;
    }

    private static LightweaveNode BuildTextNode(string text) {
        return Typography.Typography.Text.Create(
            text,
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextPrimary,
            wrap: true
        );
    }

    private static Vector2 MeasureText(string text, Rem? maxWidth) {
        if (string.IsNullOrEmpty(text)) {
            return new Vector2(new Rem(12f).ToPixels(), new Rem(1.75f).ToPixels());
        }

        float pad = new Rem(0.5f).ToPixels() * 2f;
        float maxInner = (maxWidth?.ToPixels() ?? new Rem(20f).ToPixels()) - pad;
        GameFont saved = Text.Font;
        Text.Font = GameFont.Small;
        float h = Text.CalcHeight(text, maxInner);
        Vector2 size = Text.CalcSize(text);
        Text.Font = saved;
        float width = Mathf.Min(size.x, maxInner) + pad;
        float height = h + pad;
        return new Vector2(width, height);
    }

    private static Rect ResolveScreenRect(
        Rect anchorGuiRect,
        Vector2 size,
        TooltipSide side,
        TooltipAlign align,
        float sideOffset
    ) {
        Vector2 topLeft = GUIUtility.GUIToScreenPoint(new Vector2(anchorGuiRect.x, anchorGuiRect.y));
        Vector2 bottomRight = GUIUtility.GUIToScreenPoint(new Vector2(anchorGuiRect.xMax, anchorGuiRect.yMax));
        Rect anchorScreen = new Rect(
            topLeft.x,
            topLeft.y,
            bottomRight.x - topLeft.x,
            bottomRight.y - topLeft.y
        );

        (TooltipSide cardinal, TooltipAlign effective) = ResolveSide(side, align);

        Rect candidate = PlaceTooltip(anchorScreen, size, cardinal, effective, sideOffset);
        if (FitsOnScreen(candidate)) {
            return candidate;
        }

        TooltipSide opposite = OppositeSide(cardinal);
        Rect flipped = PlaceTooltip(anchorScreen, size, opposite, effective, sideOffset);
        if (FitsOnScreen(flipped)) {
            return flipped;
        }

        return ClampToScreen(candidate);
    }

    private static (TooltipSide cardinal, TooltipAlign align) ResolveSide(TooltipSide side, TooltipAlign align) {
        return side switch {
            TooltipSide.Top => (TooltipSide.Top, align),
            TooltipSide.TopStart => (TooltipSide.Top, TooltipAlign.Start),
            TooltipSide.TopEnd => (TooltipSide.Top, TooltipAlign.End),
            TooltipSide.Right => (TooltipSide.Right, align),
            TooltipSide.RightStart => (TooltipSide.Right, TooltipAlign.Start),
            TooltipSide.RightEnd => (TooltipSide.Right, TooltipAlign.End),
            TooltipSide.Bottom => (TooltipSide.Bottom, align),
            TooltipSide.BottomStart => (TooltipSide.Bottom, TooltipAlign.Start),
            TooltipSide.BottomEnd => (TooltipSide.Bottom, TooltipAlign.End),
            TooltipSide.Left => (TooltipSide.Left, align),
            TooltipSide.LeftStart => (TooltipSide.Left, TooltipAlign.Start),
            TooltipSide.LeftEnd => (TooltipSide.Left, TooltipAlign.End),
            _ => (TooltipSide.Bottom, align),
        };
    }

    private static TooltipSide OppositeSide(TooltipSide cardinal) {
        return cardinal switch {
            TooltipSide.Top => TooltipSide.Bottom,
            TooltipSide.Bottom => TooltipSide.Top,
            TooltipSide.Left => TooltipSide.Right,
            TooltipSide.Right => TooltipSide.Left,
            _ => TooltipSide.Bottom,
        };
    }

    private static Rect PlaceTooltip(Rect anchor, Vector2 size, TooltipSide side, TooltipAlign align, float offset) {
        float x = 0f;
        float y = 0f;
        switch (side) {
            case TooltipSide.Top:
                y = anchor.y - offset - size.y;
                x = ResolveCrossAxis(anchor.x, anchor.width, size.x, align);
                break;
            case TooltipSide.Bottom:
                y = anchor.yMax + offset;
                x = ResolveCrossAxis(anchor.x, anchor.width, size.x, align);
                break;
            case TooltipSide.Left:
                x = anchor.x - offset - size.x;
                y = ResolveCrossAxis(anchor.y, anchor.height, size.y, align);
                break;
            case TooltipSide.Right:
                x = anchor.xMax + offset;
                y = ResolveCrossAxis(anchor.y, anchor.height, size.y, align);
                break;
        }

        return new Rect(x, y, size.x, size.y);
    }

    private static float ResolveCrossAxis(float anchorStart, float anchorSize, float size, TooltipAlign align) {
        return align switch {
            TooltipAlign.Start => anchorStart,
            TooltipAlign.Center => anchorStart + (anchorSize - size) / 2f,
            TooltipAlign.End => anchorStart + anchorSize - size,
            _ => anchorStart,
        };
    }

    private static bool FitsOnScreen(Rect r) {
        return r.x >= 0f && r.y >= 0f && r.xMax <= Screen.width && r.yMax <= Screen.height;
    }

    private static Rect ClampToScreen(Rect r) {
        float x = Mathf.Clamp(r.x, 0f, Mathf.Max(0f, Screen.width - r.width));
        float y = Mathf.Clamp(r.y, 0f, Mathf.Max(0f, Screen.height - r.height));
        return new Rect(x, y, r.width, r.height);
    }
}
