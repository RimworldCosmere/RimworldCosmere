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

        // A kandra the game generated arrives with the xenotype and nothing in it. Give it the
        // middle Blessing rather than letting the first check unmake it.
        if (KandraUtility.HasBlessing(pawn)) return;
        if (KandraUtility.Blessings.Count == 0) return;

        KandraUtility.GiveBlessing(pawn, HediffDefOf.Cosmere_Scadrial_Hediff_BlessingOfPresence);
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;

        KandraUtility.ReconcileSpikes(pawn);
    }
}
