using System;
using UnityEngine;

namespace Cosmere.Core.UI.Skin;

public static class SkinFontCache {
    private static readonly Dictionary<string, Font?> cache = new Dictionary<string, Font?>();

    public static Font? Resolve(string[] families, int pixelSize) {
        if (families == null || families.Length == 0) return null;
        string key = string.Join("|", families) + ":" + pixelSize;
        if (cache.TryGetValue(key, out Font? cached)) return cached;

        for (int i = 0; i < families.Length; i++) {
            string family = families[i];
            if (string.IsNullOrEmpty(family)) continue;
            Font font;
            try {
                font = Font.CreateDynamicFontFromOSFont(family, pixelSize);
            } catch (Exception ex) {
                Logger.Verbose(
                    $"SkinFontCache: CreateDynamicFontFromOSFont('{family}') threw {ex.GetType().Name}: {ex.Message}"
                );
                continue;
            }

            if (font != null && font.dynamic) {
                Logger.Info($"SkinFontCache: resolved '{family}' @ {pixelSize}px");
                cache[key] = font;
                return font;
            }
        }

        Logger.Verbose($"SkinFontCache: no font resolved for {key}; using fallback");
        cache[key] = null;
        return null;
    }

    public static void Clear() {
        cache.Clear();
    }
}
