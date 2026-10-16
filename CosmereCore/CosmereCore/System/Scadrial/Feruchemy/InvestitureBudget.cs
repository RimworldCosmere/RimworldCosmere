using System;

namespace Cosmere.System.Scadrial.Feruchemy;

/// <summary>
///     How much charge a pawn's own Investiture will bear moving, in either direction.
/// </summary>
/// <remarks>
///     Verse-free so the test host can load it. Nicrosil stores Investiture itself, so unlike a
///     metal storing something notional, the pawn runs out before the metalmind does.
/// </remarks>
public static class InvestitureBudget {
    /// <summary>Charge the pawn still has in them to give. You cannot store what you no longer have.</summary>
    /// <returns>Charge the pawn can still store, or 0 when a charge is worth no Investiture.</returns>
    public static float Storable(float currentLevel, float beuPerCharge) {
        if (beuPerCharge <= 0f) return 0f;

        return Math.Max(0f, currentLevel) / beuPerCharge;
    }

    /// <summary>The mirror of that floor: charge the pawn has room to take back.</summary>
    /// <returns>Charge the pawn has room to take back, or 0 when a charge is worth no Investiture.</returns>
    public static float Tappable(float currentLevel, float maxLevel, float beuPerCharge) {
        if (beuPerCharge <= 0f) return 0f;

        return Math.Max(0f, maxLevel - currentLevel) / beuPerCharge;
    }

    /// <summary>What a transfer of this much charge is worth to the need.</summary>
    /// <returns>The Investiture, in BEUs, that this much charge moves.</returns>
    public static float BeuFor(float charge, float beuPerCharge) {
        return charge * beuPerCharge;
    }
}
