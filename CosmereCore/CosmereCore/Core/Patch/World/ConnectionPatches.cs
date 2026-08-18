using Concord;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Entity;
using Verse;

namespace Cosmere.Core.Patch.World;

[Patch(typeof(PawnGenerator))]
public static class ConnectionPatch {
    [Inject(At.Return, nameof(PawnGenerator.GeneratePawn), parameterTypes: [typeof(PawnGenerationRequest)])]
    private static void AfterGeneratePawn(ControlHandle<Pawn> ch) {
        Pawn result = ch.ReturnValue;
        if (result == null || result.NonHumanlikeOrWildMan()) return;

        // throwaway pawns (a book's byline) are never placed; tile/layer may be null, which is fine here.
        result.GetOrCreateConnection(result.Tile.Layer);

        if (result.Faction != null && Find.World != null) {
            result.GetOrCreateConnection(result.Faction);
            foreach (Pawn pawn in Find.WorldPawns.AllPawnsAlive.Where(pawn => pawn.Faction == result.Faction)) {
                if (!pawn.Destroyed) result.GetOrCreateConnection(pawn);
            }
        }

        // TODO: layers will tie to shards eventually; grant only the pawn's layer's shards, not all.
        Shards? shards = Current.Game?.GetComponent<Shards>();
        if (shards == null) return;

        foreach (Shard? shard in shards.enabledShards.Values) {
            result.GetOrCreateConnection(shard);
        }
    }
}
