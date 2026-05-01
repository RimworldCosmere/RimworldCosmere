using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Fonts;

public static class GameFontOverride {
    public static void Apply() {
        Font? arimo = LightweaveFonts.ArimoRegular;
        if (arimo == null) {
            return;
        }

        ApplyToStyles(Text.fontStyles, arimo);
        ApplyToStyles(Text.textFieldStyles, arimo);
        ApplyToStyles(Text.textAreaStyles, arimo);
        ApplyToStyles(Text.textAreaReadOnlyStyles, arimo);
    }

    private static void ApplyToStyles(GUIStyle[] styles, Font arimo) {
        for (int i = 0; i < styles.Length; i++) {
            GUIStyle style = styles[i];
            if (style == null) {
                continue;
            }

            Font? original = style.font;
            int originalSize = original != null ? original.fontSize : style.fontSize;
            style.font = arimo;
            if (style.fontSize == 0 && originalSize > 0) {
                style.fontSize = originalSize + 2;
            }
        }
    }
}
