using Cosmere.Core.Comp.Game;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch.World;

/// A void monolith has no place in a shardworld, and it does not survive being
/// placed in one: it asks for a factionless drifter and vanilla generation throws
/// while making them, which surfaced every run as a map generation failure with
/// nothing in the stack to say what had been asked for.
///
/// Verified as vanilla by disabling every Cosmere pawn generation patch in turn -
/// the failure persisted with all of them off.
[HarmonyPatch(typeof(GenStep_Monolith), "ScatterAt")]
public static class MonolithSkipPatch {
    private static bool Prefix() {
        Shards? shards = Current.Game?.GetComponent<Shards>();

        return shards == null || shards.enabledShards.Count == 0;
    }
}
