using System;
using Cosmere.Core.ShardConnection;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Ledger;

/// <summary>
///     Duralumin's tie to a pawn's residence, sitting on top of the raw ticks
///     <see cref="ResidenceTracker" /> already keeps.
/// </summary>
public class ResidenceLedger : IConnectionLedger {
    public DuraluminLedger Ledger => DuraluminLedger.Residence;

    public float CurrentPoints(Pawn pawn) {
        return ConnectionMath.ResidenceFrom(Tracker()?.TicksFor(pawn) ?? 0);
    }

    public float HeadroomPoints(Pawn pawn) {
        return ConnectionMath.ResidenceCap - CurrentPoints(pawn);
    }

    public float Move(Pawn pawn, float points) {
        if (pawn == null || points == 0f) return 0f;

        ResidenceTracker? tracker = Tracker();
        if (tracker == null) return 0f;

        int askTicks = ConnectionMath.TicksForResidence((int)Math.Round(Math.Abs(points)));
        int movedTicks = tracker.AdjustTicks(pawn, points < 0 ? -askTicks : askTicks);
        int movedPoints = ConnectionMath.ResidenceFrom(Math.Abs(movedTicks));

        return movedTicks < 0 ? -movedPoints : movedPoints;
    }

    private static ResidenceTracker? Tracker() {
        return Current.Game?.GetComponent<ResidenceTracker>();
    }
}
