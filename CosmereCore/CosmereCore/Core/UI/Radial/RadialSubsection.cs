using UnityEngine;

namespace Cosmere.Core.UI.Radial;

public sealed record RadialSubsection(
    string SubsectionId,
    string Label,
    Texture2D? Icon,
    Color AccentColor,
    IReadOnlyList<RadialLeaf> Leaves
);
