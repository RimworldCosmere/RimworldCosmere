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
///         Keys are strings because that is the scribe shape this codebase has proven. A held
///         amount is meaningless without the Shard it was taken from, so the Shard is in the key.
///     </para>
/// </remarks>
public class ConnectionOffsets : Verse.GameComponent {
    private Dictionary<string, float> heldByPawn = [];

    public ConnectionOffsets(Verse.Game game) { }

    public override void ExposeData() {
        base.ExposeData();

        // Pawns who left the story stop mattering, and their entries would accumulate forever.
        if (Scribe.mode == LoadSaveMode.Saving) Prune();

        Scribe_Collections.Look(ref heldByPawn, "connectionOffsets", LookMode.Value, LookMode.Value);
        heldByPawn ??= [];
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
        return Current.Game?.GetComponent<ConnectionOffsets>();
    }

    private static string KeyFor(Pawn pawn, ShardDef shard) {
        return pawn.thingIDNumber + ":" + shard.defName;
    }

    /// <summary>The pawn half of a key, or -1 for anything this class did not write.</summary>
    private static int PawnIdIn(string key) {
        int split = key.IndexOf(':');

        return split > 0 && int.TryParse(key.Substring(0, split), out int id) ? id : -1;
    }

    private void Prune() {
        List<string> gone = [];
        foreach (KeyValuePair<string, float> pair in heldByPawn) {
            int id = PawnIdIn(pair.Key);
            if (id >= 0 && Find.Maps.Any(map => map.mapPawns.AllPawns.Any(p => p.thingIDNumber == id))) continue;
            gone.Add(pair.Key);
        }

        for (int i = 0; i < gone.Count; i++) heldByPawn.Remove(gone[i]);
    }
}
