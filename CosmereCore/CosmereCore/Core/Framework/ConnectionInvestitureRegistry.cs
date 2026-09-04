using System;
using Cosmere.Core.Def;
using Verse;

namespace Cosmere.Core.Framework;

/// <summary>
///     Where each shard registers how its own powers count toward Connection.
/// </summary>
public static class ConnectionInvestitureRegistry {
    private static readonly List<IConnectionInvestitureSource> sources = [];

    public static void Register(IConnectionInvestitureSource source) {
        sources.Add(source);
    }

    /// <summary>
    ///     The largest reading any shard gives, not the sum.
    /// </summary>
    /// <remarks>
    ///     A Mistborn is also a Misting of every metal, so adding the readings would count the
    ///     same Investiture ten times over and put a starting pawn at Ascendant. Taking the
    ///     highest keeps a Mistborn at Mistborn.
    /// </remarks>
    public static int StrengthFor(Pawn? pawn, ShardDef? shard) {
        if (pawn == null || shard == null) return 0;

        int best = 0;
        for (int i = 0; i < sources.Count; i++) {
            try {
                int reading = sources[i].InvestitureStrength(pawn, shard);
                if (reading > best) best = reading;
            } catch (Exception ex) {
                Log.Warn(
                    $"ConnectionInvestitureRegistry: source {sources[i].GetType().Name} threw: {ex}"
                );
            }
        }

        return best;
    }
}
