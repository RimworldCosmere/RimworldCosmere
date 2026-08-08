using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.Gene;

/// <summary>
///     Holds a kandra to its spikes, and watches what happens as they come out.
/// </summary>
/// <remarks>
///     A Blessing is a pair, and the spikes are the thing that physically exists. Pulling one is
///     a different event from pulling both, so this counts them rather than watching for the
///     Blessing hediff to vanish.
///     <list type="bullet">
///         <item>Two spikes: a kandra.</item>
///         <item>One spike: still someone, with holes in them.</item>
///         <item>None: a mistwraith, and it does not come back on its own.</item>
///     </list>
///     The check is slow on purpose. Spikes come out through surgery or a hemalurgist, both of
///     which are events rather than something that happens between two ticks.
/// </remarks>
public class BlessingBound : Verse.Gene {
    public override void PostAdd() {
        base.PostAdd();

        if (KandraUtility.Blessings.Count == 0) return;

        // A kandra the game generated arrives with the xenotype and nothing in it. One that came
        // from a save or a dev tool may have the Blessing hediff and no spikes under it, which
        // reads as a kandra with nothing holding it together: no spike to pull, and the next
        // check would turn it into a mistwraith. Either way, make the spikes match the Blessing.
        Verse.Hediff? existing = KandraUtility.BlessingOn(pawn);
        KandraUtility.GiveBlessing(
            pawn,
            existing?.def ?? HediffDefOf.Cosmere_Scadrial_Hediff_BlessingOfPresence
        );
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;

        KandraUtility.ReconcileSpikes(pawn);
    }
}
