using Cosmere.Core.Def;
using Cosmere.Core.Framework;
using Cosmere.Core.ShardConnection;
using Cosmere.System.Scadrial.Hemalurgy;
using Verse;

namespace Cosmere.System.Scadrial.Connection;

/// <summary>
///     Tells Core what a Scadrian's own powers are worth toward Ruin and Preservation.
/// </summary>
/// <remarks>
///     Core cannot ask "is this pawn a Mistborn" - it has never heard of Mistborn, and the
///     layering rule keeps it that way. This is the answer to that question, handed over through
///     a registry.
///     <para>
///         Allomancy is Preservation's and Feruchemy is both, so a Feruchemist reads against
///         Ruin as well. Harmony is not answered for directly: Core derives it from the two.
///     </para>
/// </remarks>
public class ScadrialInvestitureSource : IConnectionInvestitureSource {
    public int InvestitureStrength(Pawn pawn, ShardDef shard) {
        if (shard.defName is not ("Preservation" or "Ruin")) return 0;

        // above the genes guard: spikes live in the body, not the genome; Ruin only or Preservation drags Harmony up
        int spikes = shard.defName == "Ruin"
            ? ConnectionMath.StrengthFromSpikes(HemalurgicSpikeUtility.SpikeCount(pawn))
            : 0;

        if (pawn.genes == null) return spikes;

        bool full = pawn.IsMistborn() || pawn.IsFullFeruchemist();
        int invested = full
            ? ConnectionMath.FullInvestitureBonus
            : HasAnySingleInvestiture(pawn) ? ConnectionMath.SingleInvestitureBonus : 0;

        // the registry takes the highest reading, not the sum - a Mistborn kandra reads as whichever is worth more
        return global::System.Math.Max(spikes, invested);
    }

    /// <summary>
    ///     Whether the pawn is a Misting or a Ferring of anything at all. One metal is enough;
    ///     the registry takes the highest reading rather than the sum, so a Mistborn does not
    ///     count ten times over.
    /// </summary>
    private static bool HasAnySingleInvestiture(Pawn pawn) {
        List<MetalDef> metals = DefDatabase<MetalDef>.AllDefsListForReading;
        for (int i = 0; i < metals.Count; i++) {
            if (pawn.IsMisting(metals[i]) || pawn.IsFerring(metals[i])) return true;
        }

        return false;
    }
}
