namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     The arithmetic an eruption runs on: how far it drives the ashfall, when across its run the
///     vents throw and the ground moves, and how hard a tremor lands. Verse-free, because every
///     number here is one somebody will want to move and none of them can be checked in a live game.
/// </summary>
public static class AshEruption {
    /// <summary>
    ///     How far above the arc's standing pressure an eruption drives severity. At the Final
    ///     Empire's 0.15 this reaches 0.50, which the tracker's shaping curve turns into 8.2 times
    ///     the fall rate - the eruption is the ash arriving faster, not a different kind of ash.
    /// </summary>
    public const float PeakStep = 0.35f;

    /// <summary>
    ///     Throws one vent makes across the whole eruption, evenly spaced. With the tremors that is a
    ///     beat every 42 game minutes on the shortest roll and every 85 on the longest.
    /// </summary>
    public const int ThrowsPerVent = 25;

    /// <summary>
    ///     Times the ground moves across the whole eruption. Shares no factor with the throws, or
    ///     every tremor would land inside one and stop reading as its own event.
    /// </summary>
    public const int TremorCount = 9;

    /// <summary>Cells from a vent's mouth a tremor reaches. Well past the apron the vent sweeps.</summary>
    public const float TremorRadiusCells = 8f;

    /// <summary>
    ///     Hit points one tremor takes off something standing on the mouth itself. Five a ring out to
    ///     the edge, which is a tenth of a wooden wall for anything built against the vent's clearing.
    /// </summary>
    public const int TremorPeakDamage = 40;

    /// <summary>
    ///     Camera shake a throw beat asks for. What vanilla spends on a one-cell building falling
    ///     over, because this fires two dozen times a run and a heavy one is the first thing modded out.
    /// </summary>
    public const float ThrowShake = 0.07f;

    /// <summary>
    ///     Camera shake a tremor asks for. Between vanilla's own figures for a 2x2 and a 3x3 coming
    ///     down, so the ground moving reads as the bigger beat without saturating the camera's ceiling.
    /// </summary>
    public const float TremorShake = 0.15f;

    /// <summary>Where severity sits while the mountain is open, given what it sat at before.</summary>
    public static float SpikeSeverity(float standing) {
        float spike = standing + PeakStep;
        if (spike < 0f) return 0f;

        return spike > 1f ? 1f : spike;
    }

    /// <summary>
    ///     Whether this tick is one of <paramref name="count" /> evenly spaced beats across a run of
    ///     <paramref name="durationTicks" />. Derived from elapsed ticks rather than counted, so an
    ///     eruption carries no tally of its own and a reload cannot lose one.
    /// </summary>
    public static bool DueOn(int ticksPassed, int durationTicks, int count) {
        if (ticksPassed < 0 || durationTicks <= 0 || count <= 0) return false;

        int interval = durationTicks / count;
        if (interval < 1) interval = 1;
        if (ticksPassed % interval != 0) return false;

        // ceiling matters - an evenly dividing duration would otherwise buy a free extra beat on the closing tick.
        return ticksPassed / interval < count;
    }

    /// <summary>Damage one tremor does this far from a mouth. Linear down to nothing at the edge.</summary>
    public static int TremorDamage(float distance, float radius, int peak) {
        if (distance < 0f || radius <= 0f || peak <= 0) return 0;
        if (distance >= radius) return 0;

        return (int)(peak * (1f - distance / radius));
    }
}
