namespace Cosmere.Core.Settings.Model;

public static class SettingValueMath {
    public static float ClampFinite(float value, float fallback, float min, float max) {
        if (float.IsNaN(value) || float.IsInfinity(value)) value = fallback;
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public static void SetOrderedMinimum(int value, ref int minimum, ref int maximum, int hardMin, int hardMax) {
        minimum = Clamp(value, hardMin, hardMax);
        if (maximum < minimum) maximum = minimum;
    }

    public static void SetOrderedMaximum(int value, ref int minimum, ref int maximum, int hardMin, int hardMax) {
        maximum = Clamp(value, hardMin, hardMax);
        if (minimum > maximum) minimum = maximum;
    }

    public static void SetOrderedMinimum(
        float value,
        ref float minimum,
        ref float average,
        ref float maximum,
        float hardMin,
        float hardMax
    ) {
        minimum = ClampFinite(value, minimum, hardMin, hardMax);
        if (average < minimum) average = minimum;
        if (maximum < average) maximum = average;
    }

    public static void SetOrderedAverage(
        float value,
        ref float minimum,
        ref float average,
        ref float maximum,
        float hardMin,
        float hardMax
    ) {
        average = ClampFinite(value, average, hardMin, hardMax);
        if (minimum > average) minimum = average;
        if (maximum < average) maximum = average;
    }

    public static void SetOrderedMaximum(
        float value,
        ref float minimum,
        ref float average,
        ref float maximum,
        float hardMin,
        float hardMax
    ) {
        maximum = ClampFinite(value, maximum, hardMin, hardMax);
        if (average > maximum) average = maximum;
        if (minimum > average) minimum = average;
    }

    private static int Clamp(int value, int min, int max) {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}
