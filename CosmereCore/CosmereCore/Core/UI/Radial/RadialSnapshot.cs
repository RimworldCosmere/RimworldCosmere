using Verse;

namespace Cosmere.Core.UI.Radial;

public sealed record RadialSnapshot(
    Pawn Pawn,
    IReadOnlyList<RadialSystem> Systems,
    int BuiltAtTick
);
