using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class FeruchemySkin : ISystemSkin {
    public string SystemId => "Feruchemy";
    public string HeaderLabel => "CC_System_Feruchemy_Header".Translate();
    public Color AccentColor => new Color(0.65f, 0.38f, 0.24f);
    public Color BarFillColor => new Color(0.80f, 0.45f, 0.28f);
    public Color BarBackgroundColor => new Color(0.12f, 0.07f, 0.04f);
    public Color HeaderTextColor => new Color(0.94f, 0.75f, 0.56f);
    public GameFont HeaderFont => GameFont.Small;
    public SkinTypography Typography => SkinTypography.Empty;
    public Color PanelBackgroundColor => new Color(0.08f, 0.05f, 0.03f, 0.85f);
    public Color BorderTintColor => new Color(0.65f, 0.38f, 0.24f);
    public Texture2D? Sigil => null;
    public Texture2D? BorderFrame => null;

    public Color? GetColor(ThemeSlot slot) => slot switch {
        ThemeSlot.SurfaceAccent => AccentColor,
        ThemeSlot.TextOnAccent => HeaderTextColor,
        _ => null,
    };
    public Font? GetFont(FontRole role) => null;
    public Font? DisplayFont => null;
}
