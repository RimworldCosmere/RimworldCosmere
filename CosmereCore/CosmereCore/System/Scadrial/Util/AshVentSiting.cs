namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     Where a vent may stand and how many a map gets. Pure so the rules can be tested without a
///     map, which is the only way to check a decision the player can never undo.
/// </summary>
public static class AshVentSiting {
    /// <summary>Mean elevation over a radius-10 disc, below which the ground is too low.</summary>
    public const float MinElevation = 0.62f;

    /// <summary>Above this the cell is farmland, and a vent there is just cruel.</summary>
    public const float MaxFertility = 0.5f;

    public const int MinVents = 1;
    public const int MaxVents = 6;

    public static bool IsPlausible(float meanElevation, bool rockNearby, float fertility, bool isWater) {
        if (isWater) return false;
        if (!rockNearby) return false;
        if (fertility > MaxFertility) return false;

        return meanElevation >= MinElevation;
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
