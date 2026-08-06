namespace Cosmere.System.Scadrial.Grid;

/// <summary>
///     The pure half of the ash simulation. No Unity, no Verse - the test project cannot load
///     either, and this is the part worth guarding with tests.
/// </summary>
public static class AshDepthMath {
    public const int DustingMm = 60;
    public const int AnkleMm = 250;
    public const int KneeMm = 700;
    public const int WaistMm = 1400;

    /// <summary>Items under this much ash are buried: unhaulable and hidden.</summary>
    public const int BuriedMm = 900;

    /// <summary>Hysteresis on the way back out, or a drifting boundary flickers.</summary>
    public const int UncoveredMm = 300;

    /// <summary>Past this a cell stops being dusted ground and becomes ash terrain outright.</summary>
    public const int TerrainSwapMm = 1100;

    /// <summary>And back below this it reverts, well clear of the swap point.</summary>
    public const int TerrainRestoreMm = 600;

    public static AshBucket BucketFor(int millimetres) {
        if (millimetres >= WaistMm) return AshBucket.Waist;
        if (millimetres >= KneeMm) return AshBucket.Knee;
        if (millimetres >= AnkleMm) return AshBucket.Ankle;
        if (millimetres >= DustingMm) return AshBucket.Dusting;

        return AshBucket.None;
    }

    /// <summary>Extra ticks a pawn spends crossing a cell at this depth.</summary>
    public static int MovementTicksAddOn(AshBucket bucket) {
        return bucket switch {
            AshBucket.Dusting => 0,
            AshBucket.Ankle => 2,
            AshBucket.Knee => 8,
            AshBucket.Waist => 22,
            _ => 0,
        };
    }

    /// <summary>
    ///     Millimetres per hour at a given severity. Deliberately not linear - the Final Empire is
    ///     a nuisance a single sweeper keeps up with, and Hero of Ages is not.
    /// </summary>
    public static float FallRateMmPerHour(float severity, float ratePerDayAtFull) {
        float shaped = severity * severity * (3f - 2f * severity);
        return shaped * ratePerDayAtFull / 24f;
    }

    /// <summary>
    ///     Severity never jumps to its target. A progression beat lands over days so the player
    ///     reads it as the world thickening rather than as a cut.
    /// </summary>
    public static float EaseSeverity(float current, float target, float perDayCap, float daysElapsed) {
        float maxDelta = perDayCap * daysElapsed;
        float diff = target - current;
        if (diff > maxDelta) return current + maxDelta;
        if (diff < -maxDelta) return current - maxDelta;

        return target;
    }

    public static bool ShouldSwapToAshTerrain(int millimetres, bool alreadySwapped) {
        return alreadySwapped ? millimetres > TerrainRestoreMm : millimetres >= TerrainSwapMm;
    }

    public static bool IsBuried(int millimetres, bool alreadyBuried) {
        return alreadyBuried ? millimetres > UncoveredMm : millimetres >= BuriedMm;
    }
}
