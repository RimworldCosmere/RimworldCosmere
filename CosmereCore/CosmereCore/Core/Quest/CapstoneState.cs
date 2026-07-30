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
