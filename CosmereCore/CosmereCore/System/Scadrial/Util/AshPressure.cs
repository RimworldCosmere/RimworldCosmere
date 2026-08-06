namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     How hard the Ashmounts are pushing, 0 to 1. Game-scoped rather than per-map, so a colony
///     founded halfway through Hero of Ages starts under the ash the arc has already reached
///     instead of at the Final Empire baseline.
/// </summary>
public static class AshPressure {
    /// <summary>Where the Final Empire sits before any progression beat touches it.</summary>
    public const float Default = 0.15f;

    private static float target = Default;

    public static float Target {
        get => target;
        set => target = value < 0f ? 0f : value > 1f ? 1f : value;
    }

    public static void Reset() {
        target = Default;
    }
}
