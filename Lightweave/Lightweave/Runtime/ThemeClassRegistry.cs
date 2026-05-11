using System.Collections.Generic;

namespace Cosmere.Lightweave.Runtime;

public static class ThemeClassRegistry {
    private static readonly Dictionary<string, Style> _defaults = new();

    public static void Register(string className, Style style) {
        _defaults[className] = style;
    }

    public static Style? Get(string className) {
        if (_defaults.TryGetValue(className, out Style s)) {
            return s;
        }
        return null;
    }

    public static Style Resolve(string? baseClass, IReadOnlyList<string>? classes, Style? inline) {
        Style merged = default;
        if (baseClass != null && _defaults.TryGetValue(baseClass, out Style baseStyle)) {
            merged = baseStyle;
        }
        if (classes != null) {
            int n = classes.Count;
            for (int i = 0; i < n; i++) {
                if (_defaults.TryGetValue(classes[i], out Style s)) {
                    merged = Style.Merge(merged, s);
                }
            }
        }
        if (inline.HasValue) {
            merged = Style.Merge(merged, inline.Value);
        }
        return merged;
    }
}
