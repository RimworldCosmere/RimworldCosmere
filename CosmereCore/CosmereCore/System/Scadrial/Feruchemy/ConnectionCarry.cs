using System;

namespace Cosmere.System.Scadrial.Feruchemy;

/// <summary>
///     The bank a duralumin dial fills between transfers. The ledgers move whole points and the dial
///     pays out a fraction of one a second, so an ask waits here until it is worth a point.
/// </summary>
/// <remarks>
///     Verse-free so the test host can load it. The bank is signed - storing adds, tapping subtracts
///     - so a change of direction walks it back through zero before it pays out again.
/// </remarks>
public static class ConnectionCarry {
    /// <summary>Banks this second's rate and returns the charge it can pay for now, debiting only what it hands over.</summary>
    public static float Bank(ref float carry, float perSecond, float budget, bool storing) {
        float rate = Math.Max(0f, perSecond);
        carry += storing ? rate : -rate;

        float whole = ConnectionBudget.WholeCharge(carry);
        if (storing ? whole <= 0f : whole >= 0f) return 0f;

        float ask = ConnectionBudget.WholeCharge(Math.Min(Math.Abs(whole), Math.Max(0f, budget)));
        carry -= storing ? ask : -ask;

        return ask;
    }
}
