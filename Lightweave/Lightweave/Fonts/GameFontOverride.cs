using System.Collections.Generic;
using Cosmere.Lightweave.Settings;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Fonts;

public static class GameFontOverride {
    private static readonly Dictionary<GUIStyle, int> baselineByStyle = new();

    public static void Apply() {
        Font? arimo = LightweaveFonts.ArimoRegular;
        if (arimo == null || !arimo.dynamic) {
            return;
        }

        float scale = LightweaveMod.Settings?.FontScale ?? 1f;
        ApplyToStyles(Text.fontStyles, arimo, scale);
        ApplyToStyles(Text.textFieldStyles, arimo, scale);
        ApplyToStyles(Text.textAreaStyles, arimo, scale);
        ApplyToStyles(Text.textAreaReadOnlyStyles, arimo, scale);
    }

    private static void ApplyToStyles(GUIStyle[] styles, Font arimo, float scale) {
        for (int i = 0; i < styles.Length; i++) {
            GUIStyle style = styles[i];
            if (style == null) {
                continue;
            }

            if (!baselineByStyle.TryGetValue(style, out int baseline)) {
                Font? original = style.font;
                int originalSize = original != null ? original.fontSize : style.fontSize;
                baseline = style.fontSize == 0 && originalSize > 0 ? originalSize + 2 : style.fontSize;
                baselineByStyle[style] = baseline;
            }

            style.font = arimo;
            style.fontSize = Mathf.Max(1, Mathf.RoundToInt(baseline * scale));
        }
    }
}
