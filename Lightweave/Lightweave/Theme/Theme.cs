using System;
using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Theme;

public sealed record Theme(
    IReadOnlyDictionary<ThemeSlot, Color> Colors,
    IReadOnlyDictionary<FontRole, Font> Fonts,
    IReadOnlyDictionary<RadiusScale, float> Radii,
    IReadOnlyDictionary<ElevationScale, float> Elevations
) {
    private readonly bool _validated = ValidateConstruction(Fonts);

    private static bool ValidateConstruction(IReadOnlyDictionary<FontRole, Font> fonts) {
        if (fonts == null) {
            throw new ArgumentNullException(nameof(fonts));
        }
        if (!fonts.ContainsKey(FontRole.Body)) {
            throw new ArgumentException(
                "Theme must define FontRole.Body. Themes must define at least FontRole.Body as the fallback font.",
                nameof(fonts)
            );
        }
        return true;
    }

    public Color GetColor(ThemeSlot slot) {
        return Colors.TryGetValue(slot, out Color c) ? c : Color.magenta;
    }

    public Font GetFont(FontRole role) {
        return Fonts.TryGetValue(role, out Font f) ? f : Fonts[FontRole.Body];
    }

    public float GetRadius(RadiusScale s) {
        return Radii.TryGetValue(s, out float r) ? r : 0f;
    }

    public float GetElevation(ElevationScale s) {
        return Elevations.TryGetValue(s, out float e) ? e : 0f;
    }

    public Theme With(
        IReadOnlyDictionary<ThemeSlot, Color>? colors = null,
        IReadOnlyDictionary<FontRole, Font>? fonts = null,
        IReadOnlyDictionary<RadiusScale, float>? radii = null,
        IReadOnlyDictionary<ElevationScale, float>? elevations = null
    ) {
        Dictionary<ThemeSlot, Color> newColors = new Dictionary<ThemeSlot, Color>(Colors);
        if (colors != null) {
            foreach (KeyValuePair<ThemeSlot, Color> kv in colors) {
                newColors[kv.Key] = kv.Value;
            }
        }

        Dictionary<FontRole, Font> newFonts = new Dictionary<FontRole, Font>(Fonts);
        if (fonts != null) {
            foreach (KeyValuePair<FontRole, Font> kv in fonts) {
                newFonts[kv.Key] = kv.Value;
            }
        }

        Dictionary<RadiusScale, float> newRadii = new Dictionary<RadiusScale, float>(Radii);
        if (radii != null) {
            foreach (KeyValuePair<RadiusScale, float> kv in radii) {
                newRadii[kv.Key] = kv.Value;
            }
        }

        Dictionary<ElevationScale, float> newElev = new Dictionary<ElevationScale, float>(Elevations);
        if (elevations != null) {
            foreach (KeyValuePair<ElevationScale, float> kv in elevations) {
                newElev[kv.Key] = kv.Value;
            }
        }

        return new Theme(newColors, newFonts, newRadii, newElev);
    }

}