using Cosmere.Core.Comp.Game;
using Cosmere.Core.Entity;
using HarmonyLib;
using Verse;

namespace Cosmere.Core.Patch;

[HarmonyPatch]
public static class ConnectionPatches {
    [HarmonyPatch(
        typeof(PawnGenerator),
        nameof(PawnGenerator.GeneratePawn),
        typeof(PawnGenerationRequest)
    )]
    [HarmonyPostfix]
    public static void GeneratePawnPostfix(Pawn __result) {
        if (__result.NonHumanlikeOrWildMan()) return;

        __result.InitializeConnection(__result.Tile.Layer);
        if (__result.Faction != null) {
            __result.InitializeConnection(__result.Faction);
            foreach (Pawn pawn in Find.WorldPawns.AllPawnsAlive.Where(pawn => pawn.Faction == __result.Faction)) {
                if (!pawn.Destroyed) __result.InitializeConnection(pawn);
            }
        }

        // TODO: Layers will eventually be tied to specific shards. We should loop over the shards
        // For the layer they are on, and give connection to ONLY those shards
        foreach (Shard? shard in Current.Game.GetComponent<Shards>().enabledShards.Values) {
            __result.InitializeConnection(shard);
        }
    }
}