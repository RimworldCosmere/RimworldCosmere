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
        [DocParam("Horizontal alignment within the available width.")]
        ContainerAlign align = ContainerAlign.Center,
        [DocParam("Inline style override (use MaxWidth/Padding/etc).", TypeOverride = "Style?", DefaultOverride = "null")]
        Style? style = null,
        [DocParam("Additional class names merged after the base 'container' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
        string[]? classes = null,
        [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Length? maxWidth = style?.MaxWidth;
        Style? appliedStyle = style.HasValue ? style.Value with { MaxWidth = null } : style;

        LightweaveNode node = NodeBuilder.New("Container", line, file);
        node.ApplyStyling("container", appliedStyle, classes, id);
        node.Children.Add(child);

        float MaxWidthPx(float availableWidth) {
            if (!maxWidth.HasValue) {
                return 0f;
            }
            return maxWidth.Value.ToPixels(availableWidth, availableWidth);
        }

        (float left, float top, float right, float bottom) ResolvePaddingPixels() {
            Style s = node.GetResolvedStyle();
            EdgeInsets pad = s.Padding ?? EdgeInsets.Zero;
            return pad.Resolve(RenderContext.Current.Direction);
        }

        node.MeasureWidth = () => {
            if (!child.IsInFlow()) {
                return 0f;
            }
            (float left, _, float right, _) = ResolvePaddingPixels();
            float inner = child.MeasureWidth?.Invoke() ?? 0f;
            return inner + left + right;
        };

        node.Measure = availableWidth => {
            float maxWidthPx = MaxWidthPx(availableWidth);
            float innerWidth = maxWidthPx > 0f ? Mathf.Min(availableWidth, maxWidthPx) : availableWidth;
            if (!child.IsInFlow()) {
                return 0f;
            }
            return child.Measure?.Invoke(innerWidth) ?? child.PreferredHeight ?? 0f;
        };

        node.Paint = (rect, paintChildren) => {
            float maxWidthPx = MaxWidthPx(rect.width);
            Direction dir = RenderContext.Current.Direction;
            float outer = maxWidthPx > 0f ? Mathf.Min(rect.width, maxWidthPx) : rect.width;
            float offsetX = align switch {
                ContainerAlign.Start => dir == Direction.Rtl ? rect.width - outer : 0f,
                ContainerAlign.End => dir == Direction.Rtl ? 0f : rect.width - outer,
                _ => (rect.width - outer) * 0.5f,
            };
            if (child.IsInFlow()) {
                child.MeasuredRect = new Rect(rect.x + offsetX, rect.y, outer, rect.height);
            }
            paintChildren();
        };

        return node;
    }


    

    private static LightweaveNode DocsViewport(string labelKey, Rem maxWidth, ContainerAlign align) {
        LightweaveNode block = Box.Create(
            c => c.Add(
                Text.Create(
                    (string)labelKey.Translate(),
                    style: new Style { FontFamily = FontRole.BodyBold, FontSize = new Rem(0.8125f), TextColor = ThemeSlot.TextOnAccent, TextAlign = TextAlign.Center }
                )
            ),
            style: new Style {
                Padding = EdgeInsets.Vertical(SpacingScale.Sm),
                Background = BackgroundSpec.Of(ThemeSlot.SurfaceAccent),
                Radius = RadiusSpec.All(RadiusScale.Sm),
            }
        );
        LightweaveNode contained = Container.Create(
            block,
            align: align,
            style: new Style {
                MaxWidth = maxWidth,
                Padding = EdgeInsets.Horizontal(SpacingScale.Xs),
            }
        );
        return Box.Create(
            c => c.Add(contained),
            style: new Style {
                Padding = EdgeInsets.All(new Rem(0.25f)),
                Background = BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
                Border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderSubtle),
                Radius = RadiusSpec.All(RadiusScale.Sm),
            }
        );
    }

    [DocVariant("CL_Playground_Container_Centered")]
    public static DocSample DocsCentered() {
        return new DocSample(() => 
            DocsViewport("CL_Playground_Container_Centered_Body", new Rem(8f), ContainerAlign.Center),
            useFullSource: true
        );
    }

    [DocVariant("CL_Playground_Container_Start")]
    public static DocSample DocsStart() {
        return new DocSample(() => 
            DocsViewport("CL_Playground_Container_Start_Body", new Rem(8f), ContainerAlign.Start),
            useFullSource: true
        );
    }

    [DocVariant("CL_Playground_Container_End")]
    public static DocSample DocsEnd() {
        return new DocSample(() => 
            DocsViewport("CL_Playground_Container_End_Body", new Rem(8f), ContainerAlign.End),
            useFullSource: true
        );
    }

    [DocVariant("CL_Playground_Container_Unconstrained")]
    public static DocSample DocsUnconstrained() {
        return new DocSample(() => 
            DocsViewport("CL_Playground_Container_Unconstrained_Body", default, ContainerAlign.Center),
            useFullSource: true
        );
    }


    [DocVariant("CL_Playground_Container_Responsive")]
    public static DocSample DocsResponsive() {
        LightweaveNode block = Box.Create(
            c => c.Add(
                Text.Create(
                    (string)"CL_Playground_Container_Responsive_Body".Translate(),
                    style: new Style { FontFamily = FontRole.BodyBold, FontSize = new Rem(0.8125f), TextColor = ThemeSlot.TextOnAccent, TextAlign = TextAlign.Center }
                )
            ),
            style: new Style {
                Padding = EdgeInsets.Vertical(SpacingScale.Sm),
                Background = BackgroundSpec.Of(ThemeSlot.SurfaceAccent),
                Radius = RadiusSpec.All(RadiusScale.Sm),
            }
        );
        LightweaveNode contained = Container.Create(
            block,
            align: ContainerAlign.Center,
            style: new Style {
                MaxWidth = new Rem(40f),
                Padding = EdgeInsets.Horizontal(SpacingScale.Xs),
            }
        );
        return new DocSample(() =>
            Box.Create(
                c => c.Add(contained),
                style: new Style {
                    Padding = EdgeInsets.All(new Rem(0.25f)),
                    Background = BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
                    Border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderSubtle),
                    Radius = RadiusSpec.All(RadiusScale.Sm),
                }
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => 
            DocsViewport("CL_Playground_Container_Centered_Body", new Rem(8f), ContainerAlign.Center),
            useFullSource: true
        );
    }
}

public enum ContainerAlign {
    Start,
    Center,
    End,
}
