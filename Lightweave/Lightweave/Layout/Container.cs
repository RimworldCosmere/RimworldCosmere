using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Typography.Typography;
using Verse;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Layout;

[Doc(
    Id = "container",
    Summary = "Constrains a child to a maximum width with optional alignment and inset padding.",
    WhenToUse = "Cap how wide a section grows on a wide viewport while keeping it centered or anchored.",
    SourcePath = "Lightweave/Lightweave/Layout/Container.cs",
    PreferredVariantHeight = 96f
)]
public static class Container {
    public static LightweaveNode Create(
        [DocParam("Child constrained by max width and padding.")]
        LightweaveNode child,
        [DocParam("Maximum content width. Zero means unconstrained. Accepts a Responsive<Rem> for breakpoint-driven caps.", TypeOverride = "Responsive<Rem>", DefaultOverride = "0")]
        Responsive<Rem> maxWidth = default,
        [DocParam("Inset padding around the child. Accepts a Responsive<EdgeInsets> for breakpoint-driven padding.", TypeOverride = "Responsive<EdgeInsets>", DefaultOverride = "Zero")]
        Responsive<EdgeInsets> padding = default,
        [DocParam("Horizontal alignment within the available width.")]
        ContainerAlign align = ContainerAlign.Center,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Container", line, file);
        node.Children.Add(child);

        float ResolveInnerWidth(float availableWidth) {
            Breakpoint bp = RenderContext.Current.Breakpoint;
            float maxWidthPx = maxWidth.Resolve(bp).ToPixels();
            EdgeInsets pad = padding.Resolve(bp);
            float outer = maxWidthPx > 0f ? Mathf.Min(availableWidth, maxWidthPx) : availableWidth;
            (float leftPx, float topPx, float rightPx, float bottomPx) = pad.Resolve(RenderContext.Current.Direction);
            return Mathf.Max(0f, outer - leftPx - rightPx);
        }

        node.Measure = availableWidth => {
            Breakpoint bp = RenderContext.Current.Breakpoint;
            EdgeInsets pad = padding.Resolve(bp);
            float innerWidth = ResolveInnerWidth(availableWidth);
            (float leftPx, float topPx, float rightPx, float bottomPx) = pad.Resolve(RenderContext.Current.Direction);
            float childHeight = child.Measure?.Invoke(innerWidth) ?? child.PreferredHeight ?? 0f;
            return childHeight + topPx + bottomPx;
        };

        node.Paint = (rect, paintChildren) => {
            Breakpoint bp = RenderContext.Current.Breakpoint;
            float maxWidthPx = maxWidth.Resolve(bp).ToPixels();
            EdgeInsets pad = padding.Resolve(bp);
            Direction dir = RenderContext.Current.Direction;
            float outer = maxWidthPx > 0f ? Mathf.Min(rect.width, maxWidthPx) : rect.width;
            float offsetX = align switch {
                ContainerAlign.Start => dir == Direction.Rtl ? rect.width - outer : 0f,
                ContainerAlign.End => dir == Direction.Rtl ? 0f : rect.width - outer,
                _ => (rect.width - outer) * 0.5f,
            };
            Rect outerRect = new Rect(rect.x + offsetX, rect.y, outer, rect.height);
            Rect inner = pad.Shrink(outerRect, dir);
            child.MeasuredRect = inner;
            paintChildren();
        };

        return node;
    }


    public static LightweaveNode Responsive(
        LightweaveNode child,
        Responsive<EdgeInsets> padding = default,
        ContainerAlign align = ContainerAlign.Center,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Responsive<Rem> ladder = new Responsive<Rem>(
            new Rem(0f),
            new (Breakpoint, Rem)[] {
                (Breakpoint.Sm, new Rem(40f)),
                (Breakpoint.Md, new Rem(48f)),
                (Breakpoint.Lg, new Rem(64f)),
                (Breakpoint.Xl, new Rem(80f)),
                (Breakpoint.Xxl, new Rem(96f)),
            }
        );
        return Create(child, ladder, padding, align, line, file);
    }

    private static LightweaveNode DocsViewport(string labelKey, Rem maxWidth, ContainerAlign align) {
        LightweaveNode block = Box.Create(
            EdgeInsets.Vertical(SpacingScale.Sm),
            BackgroundSpec.Of(ThemeSlot.SurfaceAccent),
            null,
            RadiusSpec.All(new Rem(0.25f)),
            c => c.Add(
                Text.Create(
                    (string)labelKey.Translate(),
                    FontRole.BodyBold,
                    new Rem(0.8125f),
                    ThemeSlot.TextOnAccent,
                    TextAlign.Center
                )
            )
        );
        LightweaveNode contained = Container.Create(
            block,
            maxWidth,
            EdgeInsets.Horizontal(SpacingScale.Xs),
            align
        );
        return Box.Create(
            EdgeInsets.All(new Rem(0.25f)),
            BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
            BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderSubtle),
            RadiusSpec.All(new Rem(0.25f)),
            c => c.Add(contained)
        );
    }

    [DocVariant("CC_Playground_Container_Centered")]
    public static DocSample DocsCentered() {
        return new DocSample(() => 
            DocsViewport("CC_Playground_Container_Centered_Body", new Rem(8f), ContainerAlign.Center),
            useFullSource: true
        );
    }

    [DocVariant("CC_Playground_Container_Start")]
    public static DocSample DocsStart() {
        return new DocSample(() => 
            DocsViewport("CC_Playground_Container_Start_Body", new Rem(8f), ContainerAlign.Start),
            useFullSource: true
        );
    }

    [DocVariant("CC_Playground_Container_End")]
    public static DocSample DocsEnd() {
        return new DocSample(() => 
            DocsViewport("CC_Playground_Container_End_Body", new Rem(8f), ContainerAlign.End),
            useFullSource: true
        );
    }

    [DocVariant("CC_Playground_Container_Unconstrained")]
    public static DocSample DocsUnconstrained() {
        return new DocSample(() => 
            DocsViewport("CC_Playground_Container_Unconstrained_Body", default, ContainerAlign.Center),
            useFullSource: true
        );
    }


    [DocVariant("CC_Playground_Container_Responsive")]
    public static DocSample DocsResponsive() {
        LightweaveNode block = Box.Create(
            EdgeInsets.Vertical(SpacingScale.Sm),
            BackgroundSpec.Of(ThemeSlot.SurfaceAccent),
            null,
            RadiusSpec.All(new Rem(0.25f)),
            c => c.Add(
                Text.Create(
                    (string)"CC_Playground_Container_Responsive_Body".Translate(),
                    FontRole.BodyBold,
                    new Rem(0.8125f),
                    ThemeSlot.TextOnAccent,
                    TextAlign.Center
                )
            )
        );
        LightweaveNode contained = Container.Responsive(
            block,
            EdgeInsets.Horizontal(SpacingScale.Xs),
            ContainerAlign.Center
        );
        return new DocSample(() => 
            Box.Create(
                EdgeInsets.All(new Rem(0.25f)),
                BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
                BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderSubtle),
                RadiusSpec.All(new Rem(0.25f)),
                c => c.Add(contained)
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => 
            DocsViewport("CC_Playground_Container_Centered_Body", new Rem(8f), ContainerAlign.Center),
            useFullSource: true
        );
    }
}

public enum ContainerAlign {
    Start,
    Center,
    End,
}
