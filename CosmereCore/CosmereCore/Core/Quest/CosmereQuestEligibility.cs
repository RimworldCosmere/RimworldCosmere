using System.Collections.Generic;

namespace Cosmere.Core.Quest;

/// <summary>
///     A quest reduced to only the fields eligibility cares about. CosmereQuestManager
///     projects a CosmereQuestDef into one of these so the filter never touches a Def.
/// </summary>
public class QuestCandidate {
    public int cooldownDays;
    public string defName = string.Empty;
    public ScadrialEra era = ScadrialEra.Any;
    public QuestKind kind = QuestKind.Repeatable;
    public int minColonists;
    public int minDaysElapsed;
    public string? requiredCapstone;
    public List<string>? requiredFlags;
    public List<string>? requiredShards;
    public float selectionWeight = 1f;
}

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

        if (!EraMatches(candidate.era, state.era)) return false;
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

    public static List<QuestCandidate> Filter(List<QuestCandidate>? candidates, QuestWorldState state) {
        List<QuestCandidate> result = new List<QuestCandidate>();
        if (candidates == null) return result;

        for (int i = 0; i < candidates.Count; i++) {
            if (IsEligible(candidates[i], state)) result.Add(candidates[i]);
        }

        return result;
    }

    private static bool EraMatches(ScadrialEra required, ScadrialEra actual) {
        return required == ScadrialEra.Any || actual == ScadrialEra.Any || required == actual;
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
