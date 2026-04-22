using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class AllomancySkin : ISystemSkin {
    public string SystemId => "Allomancy";
    public string HeaderLabel => "CC_System_Allomancy_Header".Translate();
    public Color AccentColor => new Color(0.78f, 0.55f, 0.18f);
    public Color BarFillColor => new Color(0.85f, 0.63f, 0.22f);
    public Color BarBackgroundColor => new Color(0.14f, 0.09f, 0.04f);
    public Color HeaderTextColor => new Color(0.95f, 0.82f, 0.52f);
    public GameFont HeaderFont => GameFont.Small;
    public SkinTypography Typography => SkinTypography.Empty;
    public Color PanelBackgroundColor => new Color(0.09f, 0.05f, 0.02f, 0.85f);
    public Color BorderTintColor => new Color(0.78f, 0.55f, 0.18f);
    public Texture2D? Sigil => null;
    public Texture2D? BorderFrame => null;
}
