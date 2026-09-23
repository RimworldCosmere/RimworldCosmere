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
}
