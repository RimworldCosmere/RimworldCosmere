using System;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Skin;

public readonly struct SkinTextScope : IDisposable {
    private readonly Font? previousFont;
    private readonly int previousSize;
    private readonly bool active;

    public SkinTextScope(Font? font, int pixelSize) {
        if (font == null) {
            previousFont = null;
            previousSize = 0;
            active = false;
            return;
        }

        GUIStyle style = Text.CurFontStyle;
        previousFont = style.font;
        previousSize = style.fontSize;
        style.font = font;
        style.fontSize = pixelSize;
        active = true;
    }

    public void Dispose() {
        if (!active) return;
        GUIStyle style = Text.CurFontStyle;
        style.font = previousFont;
        style.fontSize = previousSize;
    }
}
