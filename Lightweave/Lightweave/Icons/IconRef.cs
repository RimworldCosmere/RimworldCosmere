using Cosmere.Lightweave.Fonts;
using UnityEngine;

namespace Cosmere.Lightweave.Icons;

public readonly struct IconRef {
    public readonly string Glyph;
    public readonly IconFamily Family;

    public IconRef(string glyph, IconFamily family) {
        Glyph = glyph;
        Family = family;
    }

    public Font? ResolveFont() {
        return Family switch {
            IconFamily.Phosphor => LightweaveFonts.PhosphorBold,
            IconFamily.RpgAwesome => LightweaveFonts.RpgAwesome,
            _ => null,
        };
    }
}
