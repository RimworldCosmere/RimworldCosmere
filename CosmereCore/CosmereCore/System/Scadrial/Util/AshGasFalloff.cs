using System;

namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     How much of a vent's breath reaches a cell. Pure and Verse-free so the shape can be tested.
///     What that exposure costs a pawn is <see cref="AshLungMath" />'s job, deliberately not this one's.
/// </summary>
public static class AshGasFalloff {
    /// <summary>
    ///     Straight-line distance out of the vent's footprint, so the falloff rounds off the corners
    ///     instead of ringing an even-sided mouth in another square.
    /// </summary>
    public static float DistanceToMouth(int x, int z, int minX, int minZ, int maxX, int maxZ) {
        int dx = x < minX ? minX - x : x > maxX ? x - maxX : 0;
        int dz = z < minZ ? minZ - z : z > maxZ ? z - maxZ : 0;

        return (float)Math.Sqrt(dx * dx + dz * dz);
    }

    /// <summary>
    ///     Full strength on the mouth, nothing at the radius, linear between. Linear on purpose: it
    ///     puts AshLungMath's gain and recede crossover about three quarters out, so the last ring
    ///     or two is already net relief rather than a wall a pawn can stand against.
    /// </summary>
    public static float Exposure(float distance, float radius, float atMouth) {
        if (radius <= 0f || distance >= radius) return 0f;

        float exposure = distance <= 0f ? atMouth : atMouth * (1f - distance / radius);
        if (exposure < 0f) return 0f;

        // Stat offsets stack, so a mouth strength over 1 is reachable and must not read as more
        // than fully exposed.
        return exposure > 1f ? 1f : exposure;
    }
}
