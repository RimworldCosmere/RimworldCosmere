using System;
using Cosmere.Core.Util;
using Verse;

namespace Cosmere.Core.Quest;

/// <summary>
///     Where a shard hands Core the pawn facts a quest prereq needs. Core cannot see
///     Surgebinder, so Roshar registers a probe that reads the gene for it.
/// </summary>
public static class QuestComponentRegistry {
    private static readonly List<Func<Pawn, KeyValuePair<string, int>?>> bondedOrderProbes = [];

    public static void RegisterBondedOrderProbe(Func<Pawn, KeyValuePair<string, int>?> probe) {
        bondedOrderProbes.Add(probe);
    }

    /// <summary>
    ///     Adds what this pawn is bonded to into <paramref name="into" />, keeping the highest
    ///     ideal when two pawns share an order.
    /// </summary>
    public static void CollectBondedOrders(Pawn pawn, Dictionary<string, int> into) {
        for (int i = 0; i < bondedOrderProbes.Count; i++) {
            try {
                KeyValuePair<string, int>? probed = bondedOrderProbes[i](pawn);
                if (probed == null) continue;

                KeyValuePair<string, int> pair = probed.Value;
                if (into.TryGetValue(pair.Key, out int existing) && existing >= pair.Value) continue;

                into[pair.Key] = pair.Value;
            } catch (Exception ex) {
                Log.Warn($"QuestComponentRegistry: a bonded order probe threw: {ex}");
            }
        }
    }
}
