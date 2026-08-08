namespace Cosmere.System.Scadrial.Grid;

/// <summary>What the sweep owes a cell's terrain this pass.</summary>
public enum AshTerrainAction {
    Leave = 0,
    Swap = 1,
    Restore = 2,
}

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

    /// <summary>
    ///     Hysteresis on the way back out, or a drifting boundary flickers. Close under BuriedMm
    ///     so the wash that ramps from here never shows clear ground a colonist cannot haul from.
    /// </summary>
    public const int UncoveredMm = 850;

    /// <summary>Past this a cell stops being dusted ground and becomes ash terrain outright.</summary>
    public const int TerrainSwapMm = 1100;

    /// <summary>And back below this it reverts, well clear of the swap point.</summary>
    public const int TerrainRestoreMm = 600;

    /// <summary>
    ///     A terrain change recalculates path costs and pokes the region grid, so the sweep
    ///     rations them rather than flipping a whole drift in one tick.
    /// </summary>
    public const int TerrainChangesPerSweep = 8;

    /// <summary>Shortest a cell waits before its terrain changes hands, either way.</summary>
    public const int SettleMinDays = 7;

    /// <summary>And the longest. The spread is what stops the map turning over as one wave.</summary>
    public const int SettleMaxDays = 30;

    /// <summary>
    ///     Days this cell waits. Derived from the index rather than rolled, so it survives a
    ///     reload and a save scum cannot shake a different map out of it. Salted by direction so
    ///     the ground does not come back in the same pattern it went under.
    /// </summary>
    public static int SettleDelayDays(int cellIndex, bool settling) {
        // murmur3 finalizer on a knuth-scrambled index. Anything weaker bands adjacent cells
        // together and the map turns over in stripes instead of patches.
        uint hash = (uint)cellIndex * 2654435761u;
        hash ^= settling ? 0x9E3779B9u : 0x85EBCA6Bu;
        hash ^= hash >> 15;
        hash *= 2246822519u;
        hash ^= hash >> 13;

        return SettleMinDays + (int)(hash % (uint)(SettleMaxDays - SettleMinDays + 1));
    }

    /// <summary>Days the Catacendre takes to clear a cell that was buried to the cap.</summary>
    public const float DrainDays = 3f;

    /// <summary>
    ///     Millimetres a cell sheds per sweep once the ash stops. Rated off the cap rather than
    ///     the cell's own depth so a shallow cell clears sooner than a drift, which is what
    ///     thinning looks like.
    /// </summary>
    public static float DrainMmPerSweep(int maxDepthMm, float sweepsPerDay) {
        return maxDepthMm / (DrainDays * sweepsPerDay);
    }

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

    /// <summary>Keeps the swap decision out of the MapComponent, where it cannot be tested.</summary>
    public static AshTerrainAction NextTerrainAction(int millimetres, bool alreadySwapped) {
        bool wanted = ShouldSwapToAshTerrain(millimetres, alreadySwapped);
        if (wanted == alreadySwapped) return AshTerrainAction.Leave;

        return wanted ? AshTerrainAction.Swap : AshTerrainAction.Restore;
    }

    public static bool IsBuried(int millimetres, bool alreadyBuried) {
        return alreadyBuried ? millimetres > UncoveredMm : millimetres >= BuriedMm;
    }
}
