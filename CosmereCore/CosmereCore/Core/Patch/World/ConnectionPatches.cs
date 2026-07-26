using Cosmere.Core.Comp.Game;
using Cosmere.Core.Entity;
using HarmonyLib;
using Verse;

namespace Cosmere.Core.Patch.World;

[HarmonyPatch]
public static class ConnectionPatch {
    [HarmonyPatch(
        typeof(PawnGenerator),
        nameof(PawnGenerator.GeneratePawn),
        typeof(PawnGenerationRequest)
    )]
    [HarmonyPostfix]
    public static void GeneratePawnPostfix(Pawn __result) {
        if (__result == null || __result.NonHumanlikeOrWildMan()) return;

        // Pawns are generated for throwaway purposes too - a book's author byline,
        // for one - and those are never placed, so they have no tile and the world
        // may not be up yet. Nothing here is worth failing generation over.
        __result.GetOrCreateConnection(__result.Tile.Layer);

        if (__result.Faction != null && Find.World != null) {
            __result.GetOrCreateConnection(__result.Faction);
            foreach (Pawn pawn in Find.WorldPawns.AllPawnsAlive.Where(pawn => pawn.Faction == __result.Faction)) {
                if (!pawn.Destroyed) __result.GetOrCreateConnection(pawn);
            }
        }

        // TODO: Layers will eventually be tied to specific shards. We should loop over the shards
        // For the layer they are on, and give connection to ONLY those shards
        Shards? shards = Current.Game?.GetComponent<Shards>();
        if (shards == null) return;

        foreach (Shard? shard in shards.enabledShards.Values) {
            __result.GetOrCreateConnection(shard);
        }
    }
}