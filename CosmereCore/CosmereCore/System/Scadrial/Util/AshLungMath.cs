namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     How fast ash builds in a pawn's lungs, and how fast it clears. Pure and Verse-free so the
///     curve can be tuned and tested without a comp or a hediff in the loop.
/// </summary>
public static class AshLungMath {
    /// <summary>
    ///     Severity gained per hour at full unfiltered exposure. Slow enough that lingering at an
    ///     unmasked vent takes a day or two to read as a stage change, not an emergency.
    /// </summary>
    public const float GainRatePerHourAtFullExposure = 0.01f;

    /// <summary>
    ///     Severity shed per hour in clear air. Well under the gain rate, so stepping back from a
    ///     vent is relief, not an instant reset.
    /// </summary>
    public const float RecedeRatePerHour = 0.003f;

    /// <summary>
    ///     Positive while breathing ash, negative in clear air. Blends gain and recede by
    ///     effective exposure, so a half-filtered mask lands between the two, not on one side.
    /// </summary>
    public static float SeverityDeltaPerHour(float rawExposure, float filtration) {
        float effective = EffectiveExposure(rawExposure, filtration);
        return effective * GainRatePerHourAtFullExposure - (1f - effective) * RecedeRatePerHour;
    }

    /// <summary>
    ///     Raw exposure after filtration, clamped to [0,1]. Stacked equippedStatOffsets can push
    ///     filtration past 1.0, so the clamp sits on the fraction itself, not just the result.
    /// </summary>
    public static float EffectiveExposure(float rawExposure, float filtration) {
        float clearFraction = 1f - filtration;
        if (clearFraction < 0f) clearFraction = 0f;
        else if (clearFraction > 1f) clearFraction = 1f;

        float effective = rawExposure * clearFraction;
        if (effective < 0f) return 0f;
        if (effective > 1f) return 1f;

        return effective;
    }
}
