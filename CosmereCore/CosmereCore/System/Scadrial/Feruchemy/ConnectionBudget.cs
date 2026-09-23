using System;

namespace Cosmere.System.Scadrial.Feruchemy;

/// <summary>
///     How much Connection charge may move between a pawn and a duralumin metalmind, in either
///     direction.
/// </summary>
/// <remarks>
///     Verse-free so the test host can load it. Each direction is bounded at both ends: by what
///     the pawn has, and by what the ledger has room for.
/// </remarks>
public static class ConnectionBudget {
    /// <summary>900 charge fills a band and equals 100 points.</summary>
    public const int ChargePerPoint = 9;

    /// <summary>Points this ledger can hold. Social is declared but unreachable.</summary>
    public static int Capacity(DuraluminLedger ledger) {
        return ledger switch {
            DuraluminLedger.Residence => 30,
            DuraluminLedger.Shard => 40,
            _ => 0,
        };
    }

    public static float PointsForCharge(float charge) {
        return charge / ChargePerPoint;
    }

    public static float ChargeForPoints(float points) {
        return points * ChargePerPoint;
    }

    /// <summary>Charge the pawn may give: capped by what they hold and by the ledger's remaining room.</summary>
    public static float Storable(float heldPoints, float storedCharge, DuraluminLedger ledger) {
        float room = ChargeForPoints(Capacity(ledger)) - storedCharge;

        return Math.Max(0f, Math.Min(ChargeForPoints(heldPoints), room));
    }

    /// <summary>The mirror of that floor: charge the pawn may take back, capped by what is recorded and by their headroom.</summary>
    public static float Tappable(float headroomPoints, float storedCharge, DuraluminLedger ledger) {
        if (Capacity(ledger) <= 0) return 0f;

        return Math.Max(0f, Math.Min(storedCharge, ChargeForPoints(headroomPoints)));
    }

    /// <summary>
    ///     What one side of a finished transfer still owes the other, in charge. Positive when the
    ///     metalmind moved more than the pawn paid for, negative when the pawn moved more.
    /// </summary>
    public static float Settlement(float chargeMoved, float pointsMoved) {
        if (chargeMoved < 0f || pointsMoved < 0f) return 0f;

        return chargeMoved - ChargeForPoints(pointsMoved);
    }

    /// <summary>The part of a running ask worth whole points, signed like the ask. A sub-point ask is worth nothing yet.</summary>
    public static float WholeCharge(float carry) {
        return ChargeForPoints((float)Math.Truncate(carry / ChargePerPoint));
    }
}
