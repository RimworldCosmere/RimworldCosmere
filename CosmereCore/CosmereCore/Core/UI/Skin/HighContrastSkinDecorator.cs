using Cosmere.Lightweave.Tokens;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Skin;

public sealed class HighContrastSkinDecorator : ISystemSkin {
    private readonly ISystemSkin inner;

    public HighContrastSkinDecorator(ISystemSkin inner) {
        this.inner = inner;
    }

    public string SystemId => inner.SystemId;
    public string HeaderLabel => inner.HeaderLabel;

    public Color AccentColor => Saturate(inner.AccentColor, 1.35f);
    public Color BarFillColor => Saturate(inner.BarFillColor, 1.2f);
    public Color BarBackgroundColor => new Color(0f, 0f, 0f, 0.95f);
    public Color HeaderTextColor => Color.white;
    public Color PanelBackgroundColor => new Color(0f, 0f, 0f, 0.9f);
    public Color BorderTintColor => Color.white;

    public GameFont HeaderFont => GameFont.Medium;
    public SkinTypography Typography => SkinTypography.Empty;

    public Texture2D? Sigil => inner.Sigil;
    public Texture2D? BorderFrame => null;

    public Color? GetColor(ThemeSlot slot) {
        return inner.GetColor(slot);
    }

    public Font? GetFont(FontRole role) {
        return inner.GetFont(role);
    }

    public Font? DisplayFont => inner.DisplayFont;

    private static Color Saturate(Color src, float factor) {
        float max = Mathf.Max(src.r, Mathf.Max(src.g, src.b));
        if (max <= 0f) return src;
        float mul = Mathf.Min(factor, 1f / max);
        return new Color(
            Mathf.Clamp01(src.r * mul),
            Mathf.Clamp01(src.g * mul),
            Mathf.Clamp01(src.b * mul),
            src.a
        );
    }
}