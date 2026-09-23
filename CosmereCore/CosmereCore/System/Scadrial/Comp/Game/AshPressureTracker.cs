using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.Comp.Game;

/// <summary>
///     Saves the ashfall's target severity with the game. The constructor runs for every game, so
///     a fresh colony starts at the baseline and a load overwrites it from the save a moment later.
/// </summary>
public class AshPressureTracker : GameComponent {
    private float severityTarget = AshPressure.Default;

    public AshPressureTracker(Verse.Game game) {
        AshPressure.Reset();
    }

    public override void ExposeData() {
        if (Scribe.mode == LoadSaveMode.Saving) severityTarget = AshPressure.Target;

        Scribe_Values.Look(ref severityTarget, "ashSeverityTarget", AshPressure.Default);

        if (Scribe.mode == LoadSaveMode.LoadingVars) AshPressure.Target = severityTarget;
    }
}
