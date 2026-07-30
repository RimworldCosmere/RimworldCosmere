using System.Collections.Generic;

namespace Cosmere.Core.Quest;

/// <summary>Which Scadrial era a quest belongs to. Any means it fires in both.</summary>
public enum ScadrialEra {
    Any,
    PreCatacendre,
    PostCatacendre,
}

/// <summary>How a quest reaches the player.</summary>
public enum QuestKind {
    /// <summary>Offered by the storyteller on weight, re-offers after a cooldown.</summary>
    Repeatable,

    /// <summary>Fired once from a ScenarioProgressionDef event. Can burn.</summary>
    Capstone,

    /// <summary>Not offered. It arrives, and the player deals with it.</summary>
    Threat,
}

/// <summary>
///     Whether a quest belongs to the colony or to one specific pawn. Pawn-scoped quests
///     resolve a subject at offer time and thread it through every objective, reward and
///     outcome. Nothing in the Scadrial arc uses Pawn; the Roshar arc uses it heavily.
/// </summary>
public enum QuestSubject {
    Colony,
    Pawn,
}

/// <summary>
///     A plain snapshot of everything <see cref="CosmereQuestEligibility" /> needs. Built by
///     CosmereQuestManager from live game state, and constructed directly in tests. Holds no
///     Verse types on purpose.
/// </summary>
public class QuestWorldState {
    public Dictionary<string, CapstoneState> capstoneStates = new Dictionary<string, CapstoneState>();
    public HashSet<string> completedCapstones = new HashSet<string>();
    public int currentTick;
    public int daysElapsed;
    public HashSet<string> enabledShards = new HashSet<string>();
    public ScadrialEra era = ScadrialEra.Any;
    public HashSet<string> flags = new HashSet<string>();
    public int freeColonistCount;
    public Dictionary<string, int> lastOfferedTick = new Dictionary<string, int>();

    public CapstoneState StateOf(string defName) {
        return capstoneStates.TryGetValue(defName, out CapstoneState state) ? state : CapstoneState.NotFired;
    }

    public int TicksSinceOffered(string defName) {
        if (!lastOfferedTick.TryGetValue(defName, out int tick)) return int.MaxValue;
        return currentTick - tick;
    }
}
