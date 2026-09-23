using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Util;
using RimWorld;

namespace Cosmere.System.Scadrial.Incident.Worker;

/// <summary>
///     Vanilla already does the duration, the condition, the letter and the refusal to stack. This
///     adds the two checks it cannot know about: the era still has Ashmounts pushing, and this map
///     has a mouth for them to push through.
/// </summary>
public class IncidentWorker_AshmountEruption : IncidentWorker_MakeGameCondition {
    protected override bool CanFireNowSub(IncidentParms parms) {
        if (!base.CanFireNowSub(parms)) return false;
        if (parms.target is not Verse.Map map) return false;

        // The same gate the plume and the gas ride. After the Catacendre the mountains are gone.
        if (!AshEra.CanAccumulate(map)) return false;

        AshDepthTracker? tracker = map.GetComponent<AshDepthTracker>();

        return tracker is { Vents.Count: > 0 };
    }
}
