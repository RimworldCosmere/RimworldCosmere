using UnityEngine;

namespace Cosmere.Core.UI.Radial;

public sealed record RadialSystem(
    string SystemId,
    string Label,
    Texture2D? Icon,
    IReadOnlyList<RadialSubsection> Subsections
);
