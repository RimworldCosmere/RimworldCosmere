using System;
using Cosmere.Core.Util;
using Verse;

namespace Cosmere.Core.Threat;

/// <summary>
///     What the storyteller should make of a pawn, across every shard at once.
/// </summary>
/// <remarks>
///     Each shard used to add to the raid points on its own, so a Radiant who took the Mistborn
///     boon was billed twice by two systems that could not see each other.
/// </remarks>
public static class ThreatContributorRegistry {
    private static readonly List<IThreatContributor> contributors = [];

    public static IReadOnlyList<IThreatContributor> All => contributors;

    public static void Register(IThreatContributor contributor) {
        for (int i = 0; i < contributors.Count; i++) {
            if (contributors[i].SystemId == contributor.SystemId) return;
        }

        contributors.Add(contributor);
    }

    /// <summary>
    ///     What this pawn is worth as a multiple of an ordinary colonist, never below 1. A pawn
    ///     invested by two shards stacks them through DiminishingStack, so the second gift counts
    ///     for something without the pawn being billed twice over.
    /// </summary>
    public static float MultipleForPawn(Pawn pawn) {
        List<float> gains = [];
        for (int i = 0; i < contributors.Count; i++) {
            try {
                float gain = contributors[i].GainForPawn(pawn);
                if (gain > 0f) gains.Add(gain);
            } catch (Exception ex) {
                Log.Warn($"ThreatContributorRegistry: {contributors[i].SystemId} threw: {ex}");
            }
        }

        return gains.Count == 0 ? 1f : 1f + DiminishingStack.Combine(gains);
    }
}
