using Cosmere.Core.Def;
using Verse;

namespace Cosmere.Core.ShardConnection;

/// <summary>
///     How much of each pawn's tie to each Shard is currently held somewhere other than the pawn.
/// </summary>
/// <remarks>
///     Three of the four parts a tie is composed from are recomputed on every read rather than
///     stored, so there was nowhere to take anything out of. This is that place: one number per
///     pawn and Shard, subtracted from the composed total, which makes the whole tie movable
///     without any of its parts having to become storage.
///     <para>
///         Live rows are keyed by a struct so a read allocates nothing. Saved rows are keyed by a
///         string, which is the scribe shape this codebase has proven, and projected on the way.
///     </para>
/// </remarks>
public class ConnectionOffsets : Verse.GameComponent {
    private readonly Dictionary<(int, string), float> heldByPawn = [];

    private Dictionary<string, float> scribed = [];

    public ConnectionOffsets(Verse.Game game) { }

    public override void ExposeData() {
        base.ExposeData();

        // Deliberately never pruned: dropping a row hands a pawn back a tie whose charge is still stored.
        if (Scribe.mode == LoadSaveMode.Saving) ToScribeShape();

        Scribe_Collections.Look(ref scribed, "connectionOffsets", LookMode.Value, LookMode.Value);
        scribed ??= [];

        if (Scribe.mode == LoadSaveMode.LoadingVars) FromScribeShape();
    }

    /// <summary>How much of this pawn's tie to this Shard is held elsewhere, on the 0-100 scale.</summary>
    public static float Get(Pawn? pawn, ShardDef? shard) {
        ConnectionOffsets? store = Store();
        if (store == null || pawn == null || shard == null) return 0f;

        return store.heldByPawn.TryGetValue(KeyFor(pawn, shard), out float held) ? held : 0f;
    }

    /// <summary>Records how much is held elsewhere. Bounding it is the caller's job, not this one's.</summary>
    public static void Set(Pawn? pawn, ShardDef? shard, float held) {
        ConnectionOffsets? store = Store();
        if (store == null || pawn == null || shard == null) return;

        store.heldByPawn[KeyFor(pawn, shard)] = held;
    }

    /// <summary>Null outside a running game, which is the right answer: nothing is held anywhere yet.</summary>
    private static ConnectionOffsets? Store() {
        return GameComponentCache<ConnectionOffsets>.Get();
    }

    private static (int, string) KeyFor(Pawn pawn, ShardDef shard) {
        return (pawn.thingIDNumber, shard.defName);
    }

    private void ToScribeShape() {
        scribed = new Dictionary<string, float>(heldByPawn.Count);
        foreach (KeyValuePair<(int, string), float> pair in heldByPawn) {
            scribed[pair.Key.Item1 + ":" + pair.Key.Item2] = pair.Value;
        }
    }

    private void FromScribeShape() {
        heldByPawn.Clear();
        foreach (KeyValuePair<string, float> pair in scribed) {
            int split = pair.Key.IndexOf(':');
            if (split <= 0 || !int.TryParse(pair.Key.Substring(0, split), out int id)) continue;

            heldByPawn[(id, pair.Key.Substring(split + 1))] = pair.Value;
        }
    }
}
