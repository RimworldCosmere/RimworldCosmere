namespace Cosmere.Core.Ability.Autocast;

public enum AutocastAction {
    None,
    TurnOff,
    Cast,
}

public static class AutocastDecision {
    /// <summary>
    ///     What a rule should do to an ability this pass, given the ability's shape and whether the
    ///     rule's triggers pass.
    /// </summary>
    public static AutocastAction For(
        bool toggleable,
        bool active,
        bool triggersPass,
        bool releaseOnStop,
        bool targetRequired
    ) {
        // active toggle only stops or does nothing; re-casting here would queue a duplicate job.
        if (toggleable && active) {
            return !triggersPass && releaseOnStop ? AutocastAction.TurnOff : AutocastAction.None;
        }

        if (!triggersPass || targetRequired) return AutocastAction.None;

        return AutocastAction.Cast;
    }
}
