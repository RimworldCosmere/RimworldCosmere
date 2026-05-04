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
        [DocParam("Maximum content width. Zero means unconstrained.", TypeOverride = "Rem", DefaultOverride = "0")]
        Rem maxWidth = default,
        [DocParam("Inset padding around the child.", TypeOverride = "EdgeInsets?", DefaultOverride = "null")]
        EdgeInsets? padding = null,
        [DocParam("Horizontal alignment within the available width.")]
        ContainerAlign align = ContainerAlign.Center,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Container", line, file);
        node.Children.Add(child);

        float maxWidthPx = maxWidth.ToPixels();
        EdgeInsets pad = padding ?? EdgeInsets.Zero;

        float ResolveInnerWidth(float availableWidth) {
            float outer = maxWidthPx > 0f ? Mathf.Min(availableWidth, maxWidthPx) : availableWidth;
            (float leftPx, float topPx, float rightPx, float bottomPx) = pad.Resolve(RenderContext.Current.Direction);
            return Mathf.Max(0f, outer - leftPx - rightPx);
        }

        node.Measure = availableWidth => {
            float innerWidth = ResolveInnerWidth(availableWidth);
            (float leftPx, float topPx, float rightPx, float bottomPx) = pad.Resolve(RenderContext.Current.Direction);
            float childHeight = child.Measure?.Invoke(innerWidth) ?? child.PreferredHeight ?? 0f;
            return childHeight + topPx + bottomPx;
        };

        node.Paint = (rect, paintChildren) => {
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

    private static LightweaveNode DocsViewport(string labelKey, Rem maxWidth, ContainerAlign align) {
        LightweaveNode block = Box.Create(
            EdgeInsets.Vertical(SpacingScale.Sm),
            new BackgroundSpec.Solid(ThemeSlot.SurfaceAccent),
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
            new BackgroundSpec.Solid(ThemeSlot.SurfaceSunken),
            BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderSubtle),
            RadiusSpec.All(new Rem(0.25f)),
            c => c.Add(contained)
        );
    }

    [DocVariant("CC_Playground_Container_Centered")]
    public static DocSample DocsCentered() {
        return new DocSample(
            DocsViewport("CC_Playground_Container_Centered_Body", new Rem(8f), ContainerAlign.Center)
        );
    }

    [DocVariant("CC_Playground_Container_Start")]
    public static DocSample DocsStart() {
        return new DocSample(
            DocsViewport("CC_Playground_Container_Start_Body", new Rem(8f), ContainerAlign.Start)
        );
    }

    [DocVariant("CC_Playground_Container_End")]
    public static DocSample DocsEnd() {
        return new DocSample(
            DocsViewport("CC_Playground_Container_End_Body", new Rem(8f), ContainerAlign.End)
        );
    }

    [DocVariant("CC_Playground_Container_Unconstrained")]
    public static DocSample DocsUnconstrained() {
        return new DocSample(
            DocsViewport("CC_Playground_Container_Unconstrained_Body", default, ContainerAlign.Center)
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(
            DocsViewport("CC_Playground_Container_Centered_Body", new Rem(8f), ContainerAlign.Center)
        );
    }
}

public enum ContainerAlign {
    Start,
    Center,
    End,
}
