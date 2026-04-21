using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class FeruchemySkin : ISystemSkin {
    public string SystemId => "Feruchemy";
    public string HeaderLabel => "Feruchemy";
    public Color AccentColor => new Color(0.65f, 0.38f, 0.24f);
    public Color BarFillColor => new Color(0.80f, 0.45f, 0.28f);
    public Color BarBackgroundColor => new Color(0.12f, 0.07f, 0.04f);
    public Color HeaderTextColor => new Color(0.94f, 0.75f, 0.56f);
    public GameFont HeaderFont => GameFont.Small;
}
