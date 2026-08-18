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
        if (!WorldMatches(candidate.world, state)) return false;
        if (state.freeColonistCount < candidate.minColonists) return false;
        if (state.daysElapsed < candidate.minDaysElapsed) return false;

        if (!HasAll(candidate.requiredShards, state.enabledShards)) return false;
        if (!HasAll(candidate.requiredFlags, state.flags)) return false;

        // A quest whose targetFaction is absent still builds, but degrades: no site owner, no garrison draw.
        string? targetFaction = candidate.targetFaction;
        if (targetFaction != null && targetFaction.Length > 0
            && !state.presentFactions.Contains(targetFaction)) {
            return false;
        }

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
    ///     Whether this quest may arrive as a Threat. Threats bypass the storyteller's offer
    ///     pool entirely - they are not chosen from, they happen - so they need their own
    ///     predicate rather than sharing the Repeatable one.
    /// </summary>
    public static bool IsDeliverableAsThreat(QuestCandidate? candidate, QuestWorldState? state) {
        if (candidate == null) return false;
        if (candidate.kind != QuestKind.Threat) return false;
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

    /// <summary>
    ///     Whether a quest belonging to <paramref name="questWorld" /> may run on this save.
    /// </summary>
    /// <remarks>
    ///     Deliberately permissive in all three unknown cases. A quest that names no world belongs
    ///     to every world; a cross-world save reaches every shardworld; and a state with no world
    ///     yet is a save that has not chosen one, where narrowing would silently empty the pool.
    ///     Only a genuine mismatch between two known worlds turns a quest away.
    /// </remarks>
    private static bool WorldMatches(string? questWorld, QuestWorldState state) {
        if (questWorld == null || questWorld.Length == 0) return true;
        if (state.crossWorld) return true;
        if (state.world == null || state.world.Length == 0) return true;

        return questWorld == state.world;
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
