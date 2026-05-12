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

    public Length? Width { get; init; }
    public Length? Height { get; init; }
    public Length? MinWidth { get; init; }
    public Length? MinHeight { get; init; }
    public Length? MaxWidth { get; init; }
    public Length? MaxHeight { get; init; }

    public EdgeInsets? Margin { get; init; }
    public EdgeInsets? Padding { get; init; }

    public BackgroundSpec? Background { get; init; }
    public BorderSpec? Border { get; init; }
    public RadiusSpec? Radius { get; init; }
    public float? Opacity { get; init; }
    public bool? Visible { get; init; }

    public ColorRef? TextColor { get; init; }
    public FontRef? FontFamily { get; init; }
    public Rem? FontSize { get; init; }
    public FontStyle? FontWeight { get; init; }
    public TextAlign? TextAlign { get; init; }
    public Tracking? LetterSpacing { get; init; }

    public StateStyle? Hover { get; init; }
    public StateStyle? Active { get; init; }
    public StateStyle? Focus { get; init; }
    public StateStyle? Disabled { get; init; }

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
            TextColor = overrides.TextColor ?? baseStyle.TextColor,
            FontFamily = overrides.FontFamily ?? baseStyle.FontFamily,
            FontSize = overrides.FontSize ?? baseStyle.FontSize,
            FontWeight = overrides.FontWeight ?? baseStyle.FontWeight,
            TextAlign = overrides.TextAlign ?? baseStyle.TextAlign,
            LetterSpacing = overrides.LetterSpacing ?? baseStyle.LetterSpacing,
            Hover = overrides.Hover ?? baseStyle.Hover,
            Active = overrides.Active ?? baseStyle.Active,
            Focus = overrides.Focus ?? baseStyle.Focus,
            Disabled = overrides.Disabled ?? baseStyle.Disabled,
        };
    }
}

public sealed class StateStyle {
    public Style Value { get; }

    public StateStyle(Style value) {
        Value = value;
    }

    public static implicit operator StateStyle(Style s) {
        return new StateStyle(s);
    }

    public static implicit operator Style(StateStyle s) {
        return s.Value;
    }
}
