using Verse;

namespace Cosmere.Core.Ability.Autocast;

/// A dial a rule can hold. Shards register their own, so the runner can set one
/// without Core knowing what a metalmind is.
public interface IAutocastDial {
    AutocastRuleKind Kind { get; }

    /// Where the dial rests when a rule lets go of it.
    float IdleTarget { get; }

    /// The ends of the dial's travel, so Core can offer a rule the whole range
    /// without knowing what either end means.
    float MinTarget { get; }
    float MaxTarget { get; }

    bool TrySetTarget(Pawn pawn, string targetId, float value);
}

public static class AutocastDialRegistry {
    private static readonly List<IAutocastDial> dials = [];

    public static void Register(IAutocastDial dial) {
        for (int i = 0; i < dials.Count; i++) {
            if (dials[i].Kind != dial.Kind) continue;

            dials[i] = dial;
            return;
        }

        dials.Add(dial);
    }

    public static IAutocastDial? For(AutocastRuleKind kind) {
        for (int i = 0; i < dials.Count; i++) {
            if (dials[i].Kind == kind) return dials[i];
        }

        return null;
    }
}
