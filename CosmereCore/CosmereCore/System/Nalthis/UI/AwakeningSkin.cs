using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.System.Nalthis.UI;

public sealed class AwakeningSkin : ISystemSkin {
    public string SystemId => "Awakening";
    public string HeaderLabel => "Breath";
    public Color AccentColor => new Color(0.82f, 0.22f, 0.38f);
    public Color BarFillColor => new Color(0.92f, 0.32f, 0.50f);
    public Color BarBackgroundColor => new Color(0.10f, 0.02f, 0.05f);
    public Color HeaderTextColor => new Color(0.98f, 0.75f, 0.80f);
    public GameFont HeaderFont => GameFont.Small;
}
