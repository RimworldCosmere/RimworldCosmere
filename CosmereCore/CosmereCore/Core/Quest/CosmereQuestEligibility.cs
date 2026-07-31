using System.Collections.Generic;

namespace Cosmere.Core.Quest;

/// <summary>
///     Decides which quests may be offered right now. Verse-free so the rules can be tested
///     without a running game, which matters because every rule here is a bug the player
///     would see: a post-Catacendre quest in the Final Empire, or a burned capstone
///     returning.
/// </summary>
public static class CosmereQuestEligibility {
    /// <summary>Mirrors Verse.GenDate.TicksPerDay, which the test project cannot reference.</summary>
    public const int TicksPerDay = 60000;

    public static float WeightOf(QuestCandidate? candidate) {
        if (candidate == null) return 0f;
        return candidate.selectionWeight < 0f ? 0f : candidate.selectionWeight;
    }

    public static bool IsEligible(QuestCandidate? candidate, QuestWorldState? state) {
        if (candidate == null || state == null) return false;

        if (!EraMatches(candidate.eras, state.era)) return false;
        if (state.freeColonistCount < candidate.minColonists) return false;
        if (state.daysElapsed < candidate.minDaysElapsed) return false;

        if (!HasAll(candidate.requiredShards, state.enabledShards)) return false;
        if (!HasAll(candidate.requiredFlags, state.flags)) return false;

        string? requiredCapstone = candidate.requiredCapstone;
        if (requiredCapstone != null && requiredCapstone.Length > 0
            && !state.completedCapstones.Contains(requiredCapstone)) {
            return false;
        }

        if (candidate.kind == QuestKind.Capstone) {
            CapstoneState current = state.StateOf(candidate.defName);
            if (current != CapstoneState.NotFired) return false;
        }

        return CooldownElapsed(candidate, state);
    }

    /// <summary>
    ///     Whether the storyteller may offer this quest as a random incident. Capstones reach the
    ///     player through scenario progression and Threats simply arrive, so neither belongs in the
    ///     random offer pool no matter how eligible it otherwise is.
    /// </summary>
    public static bool IsOfferableByStoryteller(QuestCandidate? candidate, QuestWorldState? state) {
        if (candidate == null) return false;
        if (candidate.kind != QuestKind.Repeatable) return false;
        return IsEligible(candidate, state);
    }

    /// <summary>
    ///     Narrows candidates to the storyteller's random-offer pool. This is the storyteller path
    ///     only - CosmereQuestManager.PickWeighted is its sole caller - so it excludes Capstone and
    ///     Threat kinds via IsOfferableByStoryteller even when IsEligible would allow them. Callers
    ///     that need the general "may this quest run right now" predicate should call IsEligible
    ///     directly instead.
    /// </summary>
    public static List<QuestCandidate> Filter(List<QuestCandidate>? candidates, QuestWorldState state) {
        List<QuestCandidate> result = new List<QuestCandidate>();
        if (candidates == null) return result;

        for (int i = 0; i < candidates.Count; i++) {
            if (IsOfferableByStoryteller(candidates[i], state)) result.Add(candidates[i]);
        }

        return result;
    }

    private static bool EraMatches(List<string>? required, string? actual) {
        if (required == null || required.Count == 0) return true;
        if (actual == null || actual.Length == 0) return false;

        for (int i = 0; i < required.Count; i++) {
            if (required[i] == actual) return true;
        }

        return false;
    }

    private static bool HasAll(List<string>? required, HashSet<string>? present) {
        if (required == null || required.Count == 0) return true;
        if (present == null) return false;

        for (int i = 0; i < required.Count; i++) {
            if (!present.Contains(required[i])) return false;
        }

        return true;
    }

    private static bool CooldownElapsed(QuestCandidate candidate, QuestWorldState state) {
        if (candidate.cooldownDays <= 0) return true;

        int elapsed = state.TicksSinceOffered(candidate.defName);
        return elapsed > candidate.cooldownDays * TicksPerDay;
    }
}
