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

        // Pawns are generated for throwaway purposes too - a book's author byline,
        // for one - and those are never placed, so they have no tile and the world
        // may not be up yet. Nothing here is worth failing generation over.
        result.GetOrCreateConnection(result.Tile.Layer);

        if (result.Faction != null && Find.World != null) {
            result.GetOrCreateConnection(result.Faction);
            foreach (Pawn pawn in Find.WorldPawns.AllPawnsAlive.Where(pawn => pawn.Faction == result.Faction)) {
                if (!pawn.Destroyed) result.GetOrCreateConnection(pawn);
            }
        }

        // TODO: Layers will eventually be tied to specific shards. We should loop over the shards
        // For the layer they are on, and give connection to ONLY those shards
        Shards? shards = Current.Game?.GetComponent<Shards>();
        if (shards == null) return;

        foreach (Shard? shard in shards.enabledShards.Values) {
            result.GetOrCreateConnection(shard);
        }
    }
}
