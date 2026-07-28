using Concord;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class DebugSpawnPatch : Verse.Thing {
    [Inject(At.Return, nameof(Notify_DebugSpawned))]
    private void AfterNotify_DebugSpawned() {
        if (def.CanHaveFaction) {
            SetFactionDirect(Find.Selector.SelectedPawns.FirstOrDefault()?.Faction ?? Faction.OfPlayer);
        }
    }
}
