using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Fonts;

[StaticConstructorOnStartup]
public static class GameFontOverride {
    static GameFontOverride() {
        Font arimo = LightweaveFonts.ArimoRegular;
        if (arimo == null) {
            return;
        }

        for (int i = 0; i < Text.fontStyles.Length; i++) {
            GUIStyle style = Text.fontStyles[i];
            if (style == null) {
                continue;
            }

            Font original = style.font;
            int originalSize = original != null ? original.fontSize : style.fontSize;
            style.font = arimo;
            if (style.fontSize == 0 && originalSize > 0) {
                style.fontSize = originalSize + 2;
            }
        }

        for (int i = 0; i < Text.textFieldStyles.Length; i++) {
            GUIStyle style = Text.textFieldStyles[i];
            if (style == null) {
                continue;
            }

            Font original = style.font;
            int originalSize = original != null ? original.fontSize : style.fontSize;
            style.font = arimo;
            if (style.fontSize == 0 && originalSize > 0) {
                style.fontSize = originalSize + 2;
            }
        }

        for (int i = 0; i < Text.textAreaStyles.Length; i++) {
            GUIStyle style = Text.textAreaStyles[i];
            if (style == null) {
                continue;
            }

            Font original = style.font;
            int originalSize = original != null ? original.fontSize : style.fontSize;
            style.font = arimo;
            if (style.fontSize == 0 && originalSize > 0) {
                style.fontSize = originalSize + 2;
            }
        }

        for (int i = 0; i < Text.textAreaReadOnlyStyles.Length; i++) {
            GUIStyle style = Text.textAreaReadOnlyStyles[i];
            if (style == null) {
                continue;
            }

            Font original = style.font;
            int originalSize = original != null ? original.fontSize : style.fontSize;
            style.font = arimo;
            if (style.fontSize == 0 && originalSize > 0) {
                style.fontSize = originalSize + 2;
            }
        }
    }
}