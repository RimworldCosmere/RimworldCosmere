namespace Cosmere.Core.Util;

/// <summary>
///     Decides whether a thing's stored Investiture sits inside a range a container will accept.
/// </summary>
/// <remarks>
///     Measured as a share of the thing's own maximum, so one setting reads the same on a chip and
///     on a broam. Kept free of Verse types so it can be tested.
/// </remarks>
public static class InvestitureRange {
    /// <summary>
    ///     Whether a charge falls inside an allowed band.
    /// </summary>
    /// <param name="current">How much Investiture the thing holds.</param>
    /// <param name="max">The most it could hold.</param>
    /// <param name="minPercent">Bottom of the band, 0 to 1.</param>
    /// <param name="maxPercent">Top of the band, 0 to 1.</param>
    /// <returns>True when the thing is allowed in.</returns>
    public static bool Within(float current, float max, float minPercent, float maxPercent) {
        // nothing to measure against, so the band cannot rule it out; build materials land here.
        if (max <= 0f) return true;

        float filled = float.IsPositiveInfinity(current) ? 1f : current / max;

        return filled >= minPercent && filled <= maxPercent;
    }
}
