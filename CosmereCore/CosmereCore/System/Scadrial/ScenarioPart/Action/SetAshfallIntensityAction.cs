using Cosmere.Core.ScenarioPart;
using Cosmere.Core.ScenarioPart.Action;
using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Util;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.ScenarioPart.Action;

/// <summary>
///     Leans on the Ashmounts. Writes the target only - the tracker walks severity towards it at a
///     per-day cap, so an act boundary lands over days rather than as a jump cut.
/// </summary>
public class SetAshfallIntensityAction : ProgressionAction {
    /// <summary>0 to 1. The Final Empire sits at 0.15; the end of Hero of Ages is 1.</summary>
    public float severity = AshPressure.Default;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        // game-scoped too: a colony founded after this beat starts under the arcs current ash, not the baseline
        AshPressure.Target = severity;

        List<Verse.Map> maps = Find.Maps;
        for (int i = 0; i < maps.Count; i++) {
            maps[i].GetComponent<AshDepthTracker>()?.SetSeverityTarget(severity);
        }

        Logger.Important($"ScenarioProgression: ashfall now climbs towards {severity}.");
    }

    public override string? Describe() {
        return "CS_Progression_Effect_Ashfall"
            .Translate(Mathf.RoundToInt(severity * 100f).Named("PERCENT"))
            .Resolve();
    }
}
