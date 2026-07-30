namespace Cosmere.Core.Quest;

/// <summary>
///     Lifecycle of a one-shot capstone quest. Declining is not an outcome, only a delay,
///     so a declined capstone returns to <see cref="NotFired" /> and its cooldown decides
///     when it may be offered again.
/// </summary>
public enum CapstoneState {
    NotFired,
    Offered,
    Completed,
    Burned,
}

/// <summary>
///     Verse-free transition rules for <see cref="CapstoneState" />. Kept separate from
///     CosmereQuestManager so the lifecycle can be tested without a running game.
/// </summary>
public static class CapstoneStateMachine {
    public static bool IsTerminal(CapstoneState state) {
        return state == CapstoneState.Completed || state == CapstoneState.Burned;
    }

    public static bool CanTransition(CapstoneState from, CapstoneState to) {
        if (IsTerminal(from)) return false;

        switch (from) {
            case CapstoneState.NotFired:
                return to == CapstoneState.Offered;
            case CapstoneState.Offered:
                return to == CapstoneState.NotFired
                       || to == CapstoneState.Completed
                       || to == CapstoneState.Burned;
            default:
                return false;
        }
    }

    public static CapstoneState OnDeclined(CapstoneState current) {
        return CanTransition(current, CapstoneState.NotFired) ? CapstoneState.NotFired : current;
    }

    public static CapstoneState OnCompleted(CapstoneState current) {
        return CanTransition(current, CapstoneState.Completed) ? CapstoneState.Completed : current;
    }

    public static CapstoneState OnFailed(CapstoneState current) {
        return CanTransition(current, CapstoneState.Burned) ? CapstoneState.Burned : current;
    }
}
