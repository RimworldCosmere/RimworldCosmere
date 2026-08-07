// Not Cosmere.Core.Connection: that shadows the existing Connection type in
// Cosmere.Core.Comp.Game, and every unrelated file using it stops compiling.
namespace Cosmere.Core.ShardConnection;

/// <summary>
///     How strongly a pawn is tied to one Shard, as a named band.
/// </summary>
/// <remarks>
///     Deliberately Verse-free so the thresholds can be tested directly. Every rule here is a
///     bug the player would see: an off-world refugee burning atium, or a plain Scadrian who
///     cannot.
/// </remarks>
public enum ConnectionTier {
    /// <summary>No relationship with that Shard.</summary>
    None,

    /// <summary>Born to the world, or brushed by it. Enough to use a god metal.</summary>
    Touched,

    /// <summary>Holds power from that Shard.</summary>
    Bonded,

    /// <summary>Savants, the heavily spiked, the story-elevated.</summary>
    Invested,

    /// <summary>Holds the Shard itself.</summary>
    Ascendant,
}
