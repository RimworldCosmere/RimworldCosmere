using Cosmere.Core.UI.Skin;
using Cosmere.Lightweave.Tokens;
using UnityEngine;
using Verse;

namespace Cosmere.System.Nalthis.UI;

public sealed class AwakeningSkin : ISystemSkin {
    public string SystemId => "Awakening";
    public string HeaderLabel => "CC_System_Awakening_Header".Translate();
    public Color AccentColor => new Color(0.82f, 0.22f, 0.38f);
    public Color BarFillColor => new Color(0.92f, 0.32f, 0.50f);
    public Color BarBackgroundColor => new Color(0.10f, 0.02f, 0.05f);
    public Color HeaderTextColor => new Color(0.98f, 0.75f, 0.80f);
    public GameFont HeaderFont => GameFont.Small;
    public SkinTypography Typography => SkinTypography.Empty;
    public Color PanelBackgroundColor => new Color(0.10f, 0.02f, 0.05f, 0.85f);
    public Color BorderTintColor => new Color(0.82f, 0.22f, 0.38f);
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