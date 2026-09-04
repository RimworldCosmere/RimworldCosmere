using Cosmere.Core.ScenarioPart;
using Cosmere.Core.ScenarioPart.Action;
using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.ScenarioPart.Action;

/// <summary>
///     The Catacendre. Stops the fall and lets the grid drain, which is what unwinds the deep-ash
///     terrain a cell at a time - the swap must never come off the era flip, or metres of ash go
///     back in one frame.
/// </summary>
public class EndAshfallAction : ProgressionAction {
    public override void Execute(GameComponent_ScenarioProgression comp) {
        AshPressure.Target = 0f;

        List<Verse.Map> maps = Find.Maps;
        for (int i = 0; i < maps.Count; i++) {
            maps[i].GetComponent<AshDepthTracker>()?.BeginDrain();
        }

        Log.Info("ScenarioProgression: the ashfall has stopped. The ground drains from here.");
    }

    public override string? Describe() {
        return "CS_Progression_Effect_AshfallEnds".Translate().Resolve();
    }
}
