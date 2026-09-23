namespace Cosmere.Core.Ability.Autocast;

public enum AutocastAction {
    None,
    TurnOff,
    Cast,
}

public static class AutocastDecision {
    /// <summary>
    ///     What a rule should do to an ability this pass, given the ability's shape, whether the
    ///     rule's triggers pass, and whether autocast is the one that lit it.
    /// </summary>
    /// <param name="autocastLit">
    ///     The rule lit this ability itself. A burn the player lit is never put out.
    /// </param>
    public static AutocastAction For(
        bool toggleable,
        bool active,
        bool triggersPass,
        bool releaseOnStop,
        bool autocastLit,
        bool targetRequired
    ) {
        // active toggle only stops or does nothing; re-casting here would queue a duplicate job.
        if (toggleable && active) {
            return !triggersPass && releaseOnStop && autocastLit
                ? AutocastAction.TurnOff
                : AutocastAction.None;
        }

        if (!triggersPass || targetRequired) return AutocastAction.None;

        return AutocastAction.Cast;
    }

    /// <summary>
    ///     Whether the rule still owns the ability after this pass. Ownership is what stops
    ///     autocast putting out a burn the player lit, so it is decided here rather than inline.
    /// </summary>
    /// <param name="dormant">The rule is switched off or carries no triggers.</param>
    /// <param name="abilityFound">The pawn still has the ability.</param>
    /// <param name="toggleable">The ability sustains once lit.</param>
    /// <param name="active">The ability is lit right now.</param>
    /// <param name="wasHolding">The rule owned it before this pass.</param>
    /// <param name="action">What this pass decided to do.</param>
    public static bool NextHolding(
        bool dormant,
        bool abilityFound,
        bool toggleable,
        bool active,
        bool wasHolding,
        AutocastAction action
    ) {
        if (dormant || !abilityFound) return false;

        // an ability that is off is nobody's, so whoever lights it next owns it.
        if (!active) return action == AutocastAction.Cast && toggleable;

        if (action == AutocastAction.TurnOff) return false;

        return action == AutocastAction.Cast ? toggleable : wasHolding;
    }
}
