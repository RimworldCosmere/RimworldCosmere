using Concord;
using Cosmere.System.Scadrial.Comp.Map;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Patch.World;

/// <summary>
///     Ash deep enough to swallow a stack takes it off the haul list. Automatic hauling only -
///     a player who wants the thing back can still order the pickup by hand.
/// </summary>
[Patch(typeof(HaulAIUtility))]
public static class AshBurialHaulPatch {
    /// <summary>
    ///     Runs for every haulable on every work scan, so the guards matter: a false result and a
    ///     map with nothing buried both cost one read before this gets out of the way.
    /// </summary>
    [Inject(At.Return, nameof(HaulAIUtility.PawnCanAutomaticallyHaulFast))]
    private static void AfterPawnCanAutomaticallyHaulFast(Verse.Thing t, ControlHandle<bool> ch) {
        if (!ch.ReturnValue) return;
        if (t.Map == null || !t.Spawned) return;

        // resolved per call: tracker hands back a new AshBuriedCells at LoadingVars; caching would use stale data
        AshDepthTracker? tracker = t.Map.GetComponent<AshDepthTracker>();
        if (tracker == null || !tracker.Buried.Any) return;

        if (tracker.Buried.IsBuried(t.Map.cellIndices.CellToIndex(t.Position))) {
            ch.ReturnValue = false;
        }
    }
}
