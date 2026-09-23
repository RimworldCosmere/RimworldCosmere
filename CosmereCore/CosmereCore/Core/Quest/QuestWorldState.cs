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

    /// <summary>FactionDef names with a live faction in this world.</summary>
    public HashSet<string> presentFactions = new HashSet<string>();

    /// <summary>The CosmereWorldDef this save runs on, or null before one is chosen.</summary>
    public string? world;

    /// <summary>
    ///     True on the cross-world sentinel, where every shardworld's content is in play.
    /// </summary>
    public bool crossWorld;

    /// <summary>Order defName to that pawn's current Ideal, for every bonded Radiant.</summary>
    public Dictionary<string, int> bondedOrders = new Dictionary<string, int>();

    /// <summary>Quest defName to the thingIDNumbers it is burned for. Empty on Scadrial.</summary>
    public Dictionary<string, HashSet<int>> pawnBurns = new Dictionary<string, HashSet<int>>();

    /// <summary>
    ///     HediffDef names on the subject pawn, flattened so the rules stay free of Verse types.
    /// </summary>
    public HashSet<string> subjectHediffs = new HashSet<string>();

    /// <summary>thingIDNumber of the subject pawn, or 0 for a colony-scoped quest.</summary>
    public int subjectPawnId;

    public bool IsBurnedForPawn(string defName, int pawnId) {
        return pawnBurns.TryGetValue(defName, out HashSet<int>? burned) && burned.Contains(pawnId);
    }

    public CapstoneState StateOf(string defName) {
        return capstoneStates.TryGetValue(defName, out CapstoneState state) ? state : CapstoneState.NotFired;
    }

    public int TicksSinceOffered(string defName) {
        if (!lastOfferedTick.TryGetValue(defName, out int tick)) return int.MaxValue;
        return currentTick - tick;
    }
}
