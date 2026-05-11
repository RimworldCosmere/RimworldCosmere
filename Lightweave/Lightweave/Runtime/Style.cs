using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.Runtime;

public readonly record struct Style {
    public Position? Position { get; init; }
    public Rem? Top { get; init; }
    public Rem? Right { get; init; }
    public Rem? Bottom { get; init; }
    public Rem? Left { get; init; }
    public int? ZIndex { get; init; }

    public Rem? Width { get; init; }
    public Rem? Height { get; init; }
    public Rem? MinWidth { get; init; }
    public Rem? MinHeight { get; init; }
    public Rem? MaxWidth { get; init; }
    public Rem? MaxHeight { get; init; }

    public EdgeInsets? Margin { get; init; }
    public EdgeInsets? Padding { get; init; }

    public BackgroundSpec? Background { get; init; }
    public BorderSpec? Border { get; init; }
    public RadiusSpec? Radius { get; init; }
    public float? Opacity { get; init; }
    public bool? Visible { get; init; }
    public Display? Display { get; init; }

    public ColorRef? Color { get; init; }
    public FontRole? FontFamily { get; init; }
    public Rem? FontSize { get; init; }
    public FontStyle? FontWeight { get; init; }
    public TextAlign? TextAlign { get; init; }

    public static Style Merge(Style baseStyle, Style overrides) {
        return new Style {
            Position = overrides.Position ?? baseStyle.Position,
            Top = overrides.Top ?? baseStyle.Top,
            Right = overrides.Right ?? baseStyle.Right,
            Bottom = overrides.Bottom ?? baseStyle.Bottom,
            Left = overrides.Left ?? baseStyle.Left,
            ZIndex = overrides.ZIndex ?? baseStyle.ZIndex,
            Width = overrides.Width ?? baseStyle.Width,
            Height = overrides.Height ?? baseStyle.Height,
            MinWidth = overrides.MinWidth ?? baseStyle.MinWidth,
            MinHeight = overrides.MinHeight ?? baseStyle.MinHeight,
            MaxWidth = overrides.MaxWidth ?? baseStyle.MaxWidth,
            MaxHeight = overrides.MaxHeight ?? baseStyle.MaxHeight,
            Margin = overrides.Margin ?? baseStyle.Margin,
            Padding = overrides.Padding ?? baseStyle.Padding,
            Background = overrides.Background ?? baseStyle.Background,
            Border = overrides.Border ?? baseStyle.Border,
            Radius = overrides.Radius ?? baseStyle.Radius,
            Opacity = overrides.Opacity ?? baseStyle.Opacity,
            Visible = overrides.Visible ?? baseStyle.Visible,
            Display = overrides.Display ?? baseStyle.Display,
            Color = overrides.Color ?? baseStyle.Color,
            FontFamily = overrides.FontFamily ?? baseStyle.FontFamily,
            FontSize = overrides.FontSize ?? baseStyle.FontSize,
            FontWeight = overrides.FontWeight ?? baseStyle.FontWeight,
            TextAlign = overrides.TextAlign ?? baseStyle.TextAlign
        };
    }
}
