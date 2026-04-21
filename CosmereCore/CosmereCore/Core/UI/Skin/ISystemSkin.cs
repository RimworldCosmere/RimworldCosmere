using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Skin;

public interface ISystemSkin {
    string SystemId { get; }
    string HeaderLabel { get; }
    Color AccentColor { get; }
    Color BarFillColor { get; }
    Color BarBackgroundColor { get; }
    Color HeaderTextColor { get; }
    GameFont HeaderFont { get; }
}
