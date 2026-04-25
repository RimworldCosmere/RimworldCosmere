using System;
using Cosmere.Core.UI.Skin;
using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Skin;

public static class SystemSkinTheme {
    public static Theme.Theme Overlay(Theme.Theme baseTheme, ISystemSkin skin) {
        Dictionary<ThemeSlot, Color> colorOverrides = new Dictionary<ThemeSlot, Color>();
        foreach (ThemeSlot slot in Enum.GetValues(typeof(ThemeSlot))) {
            Color? c = skin.GetColor(slot);
            if (c.HasValue) {
                colorOverrides[slot] = c.Value;
            }
        }

        Dictionary<FontRole, Font> fontOverrides = new Dictionary<FontRole, Font>();
        foreach (FontRole role in Enum.GetValues(typeof(FontRole))) {
            Font? f = skin.GetFont(role);
            if (f != null) {
                fontOverrides[role] = f;
            }
        }

        if (skin.DisplayFont != null) {
            fontOverrides[FontRole.Display] = skin.DisplayFont;
        }

        return baseTheme.With(colorOverrides, fontOverrides);
    }
}