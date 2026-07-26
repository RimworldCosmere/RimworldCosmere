using Cosmere.System.Scadrial.Def;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

/// Each metal already carries its own colour in its def, so the table can wear
/// real copper, brass and pewter rather than one accent hue for everything.
public static class MetalPalette {
    private static readonly Dictionary<string, Color> cache = new Dictionary<string, Color>();
    private static readonly Color Fallback = new Color(0.541f, 0.518f, 0.478f);

    public static Color For(string defName) {
        if (cache.TryGetValue(defName, out Color cached)) return cached;

        MetallicArtsMetalDef? def = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(defName);
        Color color = def != null && def.color.a > 0f ? def.color : Fallback;
        cache[defName] = color;
        return color;
    }
}
