namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     One-in-N chance per exposed hour that the mists snap someone. Game-scoped rather than
///     per-map, because the pressure is Ruin's and not any one map's.
/// </summary>
public static class MistPressure {
    public const int Default = 16;

    private static int oneIn = Default;

    public static int OneIn {
        get => oneIn;
        set => oneIn = value < 1 ? 1 : value;
    }

    public static void Reset() {
        oneIn = Default;
    }
}
