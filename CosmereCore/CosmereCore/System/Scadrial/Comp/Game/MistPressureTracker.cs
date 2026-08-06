using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.Comp.Game;

/// <summary>
///     Saves the mists' snap pressure with the game. The constructor runs for every game, so a
///     fresh colony starts at the default and a load overwrites it from the save a moment later.
/// </summary>
public class MistPressureTracker : GameComponent {
    private int snapOneIn = MistPressure.Default;

    public MistPressureTracker(Verse.Game game) {
        MistPressure.Reset();
    }

    public override void ExposeData() {
        if (Scribe.mode == LoadSaveMode.Saving) snapOneIn = MistPressure.OneIn;

        Scribe_Values.Look(ref snapOneIn, "mistSnapOneIn", MistPressure.Default);

        if (Scribe.mode == LoadSaveMode.LoadingVars) MistPressure.OneIn = snapOneIn;
    }
}
