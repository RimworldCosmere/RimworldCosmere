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

    /// <summary>Throws one vent makes across the whole eruption, evenly spaced.</summary>
    public const int ThrowsPerVent = 4;

    /// <summary>Times the ground moves across the whole eruption.</summary>
    public const int TremorCount = 6;

    /// <summary>Cells from a vent's mouth a tremor reaches. Well past the apron the vent sweeps.</summary>
    public const float TremorRadiusCells = 8f;

    /// <summary>Hit points one tremor takes off something standing on the mouth itself.</summary>
    public const int TremorPeakDamage = 18;

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

        // The first beat lands the moment the mountain opens and the last stops short of the end.
        // Without the ceiling a duration that divides evenly buys a free extra beat on the closing
        // tick, because a condition is not expired until the tick after its duration runs out.
        return ticksPassed / interval < count;
    }

    /// <summary>Damage one tremor does this far from a mouth. Linear down to nothing at the edge.</summary>
    public static int TremorDamage(float distance, float radius, int peak) {
        if (distance < 0f || radius <= 0f || peak <= 0) return 0;
        if (distance >= radius) return 0;

        return (int)(peak * (1f - distance / radius));
    }
}
