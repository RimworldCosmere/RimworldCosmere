using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Theme;

public static class BaseTheme {
    public static Dictionary<FontRole, Font> BuildFonts(
        Font body,
        Font bodyBold,
        Font heading,
        Font display,
        Font mono
    ) {
        return new Dictionary<FontRole, Font> {
            [FontRole.Body] = body,
            [FontRole.BodyBold] = bodyBold,
            [FontRole.Heading] = heading,
            [FontRole.Display] = display,
            [FontRole.Label] = body,
            [FontRole.Caption] = body,
            [FontRole.Mono] = mono,
        };
    }

    public static Dictionary<RadiusScale, float> BuildRadii() {
        return new Dictionary<RadiusScale, float> {
            [RadiusScale.None] = 0f,
            [RadiusScale.Sm] = 4f,
            [RadiusScale.Md] = 8f,
            [RadiusScale.Lg] = 16f,
            [RadiusScale.Full] = 9999f,
        };
    }

    public static Dictionary<ElevationScale, float> BuildElevations() {
        return new Dictionary<ElevationScale, float> {
            [ElevationScale.Flat] = 0f,
            [ElevationScale.Sm] = 2f,
            [ElevationScale.Md] = 6f,
            [ElevationScale.Lg] = 12f,
        };
    }

    public static Theme Compose(
        Dictionary<ThemeSlot, Color> colors,
        Font body,
        Font bodyBold,
        Font heading,
        Font display,
        Font mono
    ) {
        Dictionary<FontRole, Font> fonts = BuildFonts(body, bodyBold, heading, display, mono);
        Dictionary<RadiusScale, float> radii = BuildRadii();
        Dictionary<ElevationScale, float> elev = BuildElevations();
        return new Theme(colors, fonts, radii, elev);
    }
}