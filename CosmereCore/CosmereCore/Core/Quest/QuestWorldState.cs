using System.Collections.Generic;

namespace Cosmere.Core.Quest;

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
    public string? era;
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
