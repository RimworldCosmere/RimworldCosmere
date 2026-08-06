using Cosmere.Core.Def;
using Verse;

namespace Cosmere.Core.Framework;

/// <summary>
///     Reports what a pawn's own Investiture is worth toward one Shard.
/// </summary>
/// <remarks>
///     Core has no idea what a Misting or a Full Feruchemist is, and must not - that knowledge
///     belongs to Scadrial. Each shard registers its own reading and Core only adds up what comes
///     back, so a new shardworld contributes its powers without Core changing.
/// </remarks>
public interface IConnectionInvestitureSource {
    /// <summary>
    ///     Strength on the 0-100 Connection scale, or 0 when this source has nothing to say
    ///     about that pawn and that Shard.
    /// </summary>
    int InvestitureStrength(Pawn pawn, ShardDef shard);
}
