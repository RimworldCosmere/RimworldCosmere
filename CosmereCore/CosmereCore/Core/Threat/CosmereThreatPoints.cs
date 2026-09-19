using Verse;

namespace Cosmere.Core.Threat;

/// <summary>
///     The body the storyteller injection calls once per colonist.
/// </summary>
/// <remarks>
///     Scaling the per-colonist value rather than adding at the return keeps the bonus inside
///     vanilla's own factors: adaptation, threatScale, the days ramp and the 10000 clamp.
/// </remarks>
public static class CosmereThreatPoints {
    public static float ForColonist(float points, Pawn pawn) {
        return points * ThreatContributorRegistry.MultipleForPawn(pawn);
    }
}
