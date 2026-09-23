namespace Cosmere.Core.ScenarioPart.Action;

public abstract class ProgressionAction {
    public abstract void Execute(GameComponent_ScenarioProgression comp);

    /// <summary>
    ///     One line naming what this action changes mechanically, for the letter to list under
    ///     its prose. Null for anything the player has no outcome to read - narration, handoffs,
    ///     the letter itself.
    ///     <para>
    ///         Written here rather than into each letter's text so the two cannot drift: retune
    ///         a goodwill number and the card reporting it follows automatically.
    ///     </para>
    /// </summary>
    public virtual string? Describe() {
        return null;
    }
}
