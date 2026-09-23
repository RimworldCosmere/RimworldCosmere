using System;

namespace Cosmere.System.Scadrial.Feruchemy;

/// <summary>
///     What one side of a duralumin transfer still owes the other. Signed: charge is positive into
///     the metalmind, points are positive onto the pawn.
/// </summary>
/// <remarks>
///     Verse-free so the test host can load it. Only one of the two is ever non-zero - whichever
///     side moved more is the side that hands the difference back.
/// </remarks>
public readonly struct ConnectionSettlement {
    private ConnectionSettlement(float metalmindCharge, float pawnPoints) {
        MetalmindCharge = metalmindCharge;
        PawnPoints = pawnPoints;
    }

    public float MetalmindCharge { get; }

    public float PawnPoints { get; }

    /// <summary>The signed whole-point ask the pawn's ledger gets: negative takes from them, positive gives to them.</summary>
    public static float Ask(float chargeMoved, bool storing) {
        float points = ConnectionBudget.PointsForCharge(ConnectionBudget.WholeCharge(Math.Max(0f, chargeMoved)));

        return storing ? -points : points;
    }

    /// <summary>Reads what the ledger reported back against what the metalmind moved, and decides which side settles.</summary>
    public static ConnectionSettlement Resolve(float chargeMoved, bool storing, float signedPointsMoved) {
        float pointsMoved = storing ? -signedPointsMoved : signedPointsMoved;
        float owed = ConnectionBudget.Settlement(chargeMoved, Math.Max(0f, pointsMoved));
        if (owed == 0f) return default;

        // the metalmind moved more than the pawn paid for, so that part of its move never happened.
        if (owed > 0f) return new ConnectionSettlement(storing ? -owed : owed, 0f);

        float points = ConnectionBudget.PointsForCharge(-owed);

        return new ConnectionSettlement(0f, storing ? points : -points);
    }
}
