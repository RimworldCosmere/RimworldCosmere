using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

/// <summary>
///     Hands the campaign over to the next arc. A scenario's progression is otherwise fixed at
///     game start, so without this each arc simply stops when its last beat fires and the
///     Scadrial timeline never continues past the scenario the player picked.
/// </summary>
public class HandOffProgressionAction : ProgressionAction {
    public ScenarioProgressionDef? progression;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        ScenarioProgressionDef? next = progression;
        if (next == null) {
            Logger.Warning("ScenarioProgression: HandOffProgressionAction has no progression set");
            return;
        }

        comp.HandOffTo(next);
    }
}
