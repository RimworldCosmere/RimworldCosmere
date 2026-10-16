using Cosmere.Core.Def;
using Verse;

namespace Cosmere.Core.Comp.Game;

/// <summary>
///     How strongly the colony is tied to each Shard, as a multiplier on odds that Shard
///     governs. 1.0 is the untouched baseline; above that, whatever a Shard reaches for it
///     reaches for more often.
///     <para>
///         A stopgap. It exists because the Pits payoff needs somewhere to put "the colony is
///         closer to Preservation now" and there was nowhere. The real thing wants a value the
///         player can see and other systems can read - see the Shard connection task.
///     </para>
/// </summary>
public class ShardConnections : Verse.GameComponent {
    private Dictionary<string, float> strengthByShard = [];

    public ShardConnections(Verse.Game game) { }

    public override void ExposeData() {
        Scribe_Collections.Look(ref strengthByShard, "strengthByShard", LookMode.Value, LookMode.Value);
        strengthByShard ??= [];
    }

    public float Get(ShardDef? shard) {
        if (shard == null) return 1f;
        return strengthByShard.TryGetValue(shard.defName, out float strength) ? strength : 1f;
    }

    /// <summary>Adds to the multiplier. Never drops it below the untouched baseline.</summary>
    public void Add(ShardDef? shard, float amount) {
        if (shard == null) return;

        float next = Get(shard) + amount;
        strengthByShard[shard.defName] = next < 1f ? 1f : next;
    }
}
