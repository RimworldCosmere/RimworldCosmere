namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     Where a vent may stand and how many a map gets. Pure so the rules can be tested without a
///     map, which is the only way to check a decision the player can never undo.
/// </summary>
public static class AshVentSiting {
    /// <summary>Mean elevation over a radius-10 disc, below which the ground is too low.</summary>
    public const float MinElevation = 0.62f;

    /// <summary>
    ///     Terrain def fertility at or above this is farmable, and a vent there is just cruel. The
    ///     vent then makes its own: siting runs once at map gen, so the soil never feeds back in.
    /// </summary>
    public const float MaxFertility = 0.5f;

    public const int MinVents = 1;
    public const int MaxVents = 6;

    public static bool IsPlausible(float meanElevation, bool rockNearby, float fertility, bool isWater) {
        if (!PassesCheapRules(fertility, isWater)) return false;
        if (!rockNearby) return false;

        return meanElevation >= MinElevation;
    }

    /// <summary>
    ///     The half of the rule a caller can answer without walking a grid. Split out so map gen
    ///     can reject a cell before paying for the elevation and rock discs.
    /// </summary>
    public static bool PassesCheapRules(float fertility, bool isWater) {
        if (isWater) return false;

        return fertility < MaxFertility;
    }

    /// <summary>
    ///     What survives on the fallback pass. Elevation and fertility go; a starved map is meant
    ///     to get its vent somewhere rather than nowhere.
    /// </summary>
    public static bool IsPlausibleFallback(bool isWater) {
        return !isWater;
    }

    /// <summary>
    ///     Exposure runs 1 to 3. One vent at the belt's edge, six on a mount-adjacent tile, and
    ///     the steps between are what makes distance to a mount feel like a choice.
    /// </summary>
    public static int CountForExposure(float exposure) {
        if (exposure <= 1f) return MinVents;
        if (exposure >= 3f) return MaxVents;

        int count = MinVents + (int)((exposure - 1f) / 2f * (MaxVents - MinVents) + 0.5f);
        return count < MinVents ? MinVents : count > MaxVents ? MaxVents : count;
    }
}
