namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     What sealing a vent buys the rest of the map. The ash does not stop - it comes off the
///     Ashmounts - so the relief is a share of severity, never a subtraction and never all of it.
/// </summary>
public static class AshVentRelief {
    /// <summary>
    ///     Most of a map's severity containment can ever take. Half, so the endgame's severity 1
    ///     still stands at 0.5 with every vent sealed and the mountains keep falling on you.
    /// </summary>
    public const float MaxRelief = 0.5f;

    /// <summary>
    ///     One sealed vent's share, split across the most vents a tile can carry rather than the
    ///     ones it has. A roof is worth the same wherever it goes and the sixth still pays.
    /// </summary>
    public const float PerVent = MaxRelief / AshVentSiting.MaxVents;

    /// <summary>Fraction of a map's severity its sealed vents have taken off it.</summary>
    public static float Fraction(int sealedVents) {
        if (sealedVents <= 0) return 0f;

        float relief = PerVent * sealedVents;
        return relief > MaxRelief ? MaxRelief : relief;
    }

    /// <summary>
    ///     Severity the map actually falls at. Taking a flat 0.5 off the Final Empire's 0.15 would
    ///     leave clear sky on the tiles carrying the most vents, so the relief scales instead.
    /// </summary>
    public static float Relieve(float severity, int sealedVents) {
        if (severity <= 0f) return severity;

        return severity * (1f - Fraction(sealedVents));
    }
}
