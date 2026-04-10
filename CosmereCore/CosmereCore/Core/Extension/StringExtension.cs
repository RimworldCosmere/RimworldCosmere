using UnityEngine;

namespace Cosmere.Core.Extension;

public static class StringExtension {
    public static string ColoredBool(this bool value) {
        return value ? "<color=green>true</color>" : "<color=red>false</color>";
    }

    public static string ColoredColor(this Color color) {
        string hex = ColorUtility.ToHtmlStringRGBA(color);
        return $"<color=#{hex}>{color}</color>";
    }

    public static string ColoredBreakpoints(
        this float value,
        float baseline = 1f,
        Dictionary<float, Color>? breakpoints = null
    ) {
        // Default breakpoints if none provided
        breakpoints ??= new Dictionary<float, Color> {
            { 0.25f, Color.red },
            { 0.75f, Color.yellow },
            { 1.5f, Color.green },
            { 3.0f, Color.blue },
        };

        float ratio = value / baseline;

        // Sort breakpoints by threshold
        KeyValuePair<float, Color>[] sortedBreakpoints = breakpoints.OrderBy(kvp => kvp.Key).ToArray();

        Color? color = null;

        // Find which range the ratio falls into
        if (ratio <= sortedBreakpoints[0].Key) {
            color = sortedBreakpoints[0].Value;
        } else if (ratio >= sortedBreakpoints[^1].Key) {
            color = sortedBreakpoints[^1].Value;
        } else {
            // Interpolate between two breakpoints
            for (int i = 0; i < sortedBreakpoints.Length - 1; i++) {
                float lowerThreshold = sortedBreakpoints[i].Key;
                float upperThreshold = sortedBreakpoints[i + 1].Key;

                if (!(ratio <= upperThreshold)) continue;
                float t = (ratio - lowerThreshold) / (upperThreshold - lowerThreshold);
                color = Color.Lerp(sortedBreakpoints[i].Value, sortedBreakpoints[i + 1].Value, t);
                break;
            }
        }

        color ??= sortedBreakpoints[^1].Value;

        string hex = ColorUtility.ToHtmlStringRGB(color.Value);
        return $"<color=#{hex}>{value:F2}</color>";
    }
}