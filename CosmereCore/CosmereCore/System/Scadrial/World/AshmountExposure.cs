using System.Collections.Generic;

namespace Cosmere.System.Scadrial.World;

/// <summary>
///     How buried a tile is, as a multiplier on the fall rate. Sums every mount in range rather
///     than taking the nearest, which is what buries the heartland inside the ring.
/// </summary>
public static class AshmountExposure {
    /// <summary>Past this a mount contributes nothing.</summary>
    public const float RangeTiles = 40f;

    /// <summary>Ceiling, so no tile is generated unplayable.</summary>
    public const float MaxMultiplier = 3f;

    /// <summary>One mount's share. Quadratic so the falloff is gentle near and steep far.</summary>
    public static float Contribution(float distanceInTiles) {
        if (distanceInTiles >= RangeTiles) return 0f;

        float t = 1f - distanceInTiles / RangeTiles;
        return t * t;
    }

    /// <summary>1 is an ordinary tile. Clamped at <see cref="MaxMultiplier" />.</summary>
    public static float Multiplier(IReadOnlyList<float> distancesInTiles) {
        float sum = 0f;
        for (int i = 0; i < distancesInTiles.Count; i++) {
            sum += Contribution(distancesInTiles[i]);
        }

        float multiplier = 1f + sum;
        return multiplier > MaxMultiplier ? MaxMultiplier : multiplier;
    }
}
