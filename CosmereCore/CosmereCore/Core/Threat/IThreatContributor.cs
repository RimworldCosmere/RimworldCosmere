using Verse;

namespace Cosmere.Core.Threat;

/// <summary>
///     One shard's answer to "how much more dangerous than an ordinary colonist is this pawn".
/// </summary>
public interface IThreatContributor {
    string SystemId { get; }

    /// <summary>
    ///     What this pawn's investiture adds, counted in colonists. Zero means this shard has
    ///     nothing invested in the pawn, which is the answer for most pawns on most maps.
    /// </summary>
    float GainForPawn(Pawn pawn);
}
