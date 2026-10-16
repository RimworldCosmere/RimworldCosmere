using Concord;
using RimWorld;
using Verse;

namespace Cosmere.Core.Threat;

/// <summary>
///     Scales what each invested colonist is worth to the storyteller.
/// </summary>
/// <remarks>
///     Injected where vanilla writes a colonist's own points, so the bonus rides the health lerp,
///     adaptation, threatScale, the days ramp and the 10000 clamp instead of escaping them.
/// </remarks>
[Patch(typeof(StorytellerUtility))]
public static class CosmereStorytellerUtilityPatch {
    /// <summary>
    ///     Ordinal 6 is the per-colonist points local, and store 2 is the free-colonist branch.
    ///     Store 1 is the zero it is initialised to.
    /// </summary>
    [Inject(
        nameof(StorytellerUtility.DefaultThreatPointsNow),
        typeof(float),
        LocalAccess.Store,
        At.Local,
        by: 2,
        ordinal: 6,
        parameterTypes: [typeof(IIncidentTarget)]
    )]
    private static float AfterColonistPoints(float points, [Local] Pawn pawn) {
        return CosmereThreatPoints.ForColonist(points, pawn);
    }
}
