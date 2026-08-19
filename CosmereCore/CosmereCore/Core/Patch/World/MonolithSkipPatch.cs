using Concord;
using Cosmere.Core.Comp.Game;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch.World;

/// <summary>
///     Void monoliths ask for a factionless drifter during map gen, and vanilla's
///     GenStep_Monolith throws while creating one - skip the step entirely when a shard is active.
/// </summary>
[Patch]
public abstract class MonolithSkipPatch : GenStep_Monolith {
    [Inject(At.Head, nameof(ScatterAt))]
    private Control BeforeScatterAt() {
        Shards? shards = Current.Game?.GetComponent<Shards>();

        return shards == null || shards.enabledShards.Count == 0 ? Control.Continue : Control.Cancel;
    }
}
