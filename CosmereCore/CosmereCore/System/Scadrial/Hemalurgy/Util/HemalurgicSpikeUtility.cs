using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy;

/// <summary>
///     How much of Ruin's metal a body is carrying.
/// </summary>
/// <remarks>
///     Counting spikes is a Hemalurgy question, not a kandra one. It used to live on
///     <c>KandraUtility</c> because kandra were the only thing that asked, but Connection asks now
///     too, and Core reaching into a kandra utility to find out how Invested somebody is would be
///     the wrong shape.
/// </remarks>
public static class HemalurgicSpikeUtility {
    public static int SpikeCount(Pawn? pawn) {
        Verse.Hediff? spiked = pawn?.health?.hediffSet?.GetFirstHediffOfDef(
            HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
        );

        return spiked is Hediff.HemalurgicSpikes set ? set.spikeCount : 0;
    }
}
