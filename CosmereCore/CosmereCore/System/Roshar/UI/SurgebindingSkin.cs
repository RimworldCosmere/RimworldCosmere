using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.UI;

public sealed class SurgebindingSkin : ISystemSkin {
    public string SystemId => "Surgebinding";
    public string HeaderLabel => "CC_System_Surgebinding_Header".Translate();
    public Color AccentColor => new Color(0.55f, 0.78f, 1.00f);
    public Color BarFillColor => new Color(0.70f, 0.88f, 1.00f);
    public Color BarBackgroundColor => new Color(0.03f, 0.06f, 0.12f);
    public Color HeaderTextColor => new Color(0.90f, 0.95f, 1.00f);
    public GameFont HeaderFont => GameFont.Small;
    public SkinTypography Typography => SkinTypography.Empty;
    public Color PanelBackgroundColor => new Color(0.03f, 0.06f, 0.12f, 0.85f);
    public Color BorderTintColor => new Color(0.55f, 0.78f, 1.00f);
    public Texture2D? Sigil => null;
    public Texture2D? BorderFrame => null;

    public Color? GetColor(ThemeSlot slot) {
        return slot switch {
            ThemeSlot.SurfaceAccent => AccentColor,
            ThemeSlot.TextOnAccent => HeaderTextColor,
            _ => null,
        };
    }

    public Font? GetFont(FontRole role) {
        return null;
    }

    public Font? DisplayFont => null;
}