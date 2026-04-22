using System.Collections.Generic;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Tokens;

namespace Cosmere.Core.UI.Lightweave.Theme;

public static class BaseTheme
{
    public static Theme Build(Font body, Font bodyBold, Font heading, Font display, Font mono)
    {
        Dictionary<ThemeSlot, Color> colors = new Dictionary<ThemeSlot, Color>
        {
            [ThemeSlot.SurfacePrimary] = new Color(0.10f, 0.10f, 0.12f, 0.95f),
            [ThemeSlot.SurfaceRaised]  = new Color(0.14f, 0.14f, 0.17f, 0.95f),
            [ThemeSlot.SurfaceSunken]  = new Color(0.07f, 0.07f, 0.09f, 0.95f),
            [ThemeSlot.SurfaceAccent]  = new Color(0.35f, 0.65f, 0.85f, 0.90f),
            [ThemeSlot.TextPrimary]    = new Color(0.95f, 0.95f, 0.95f),
            [ThemeSlot.TextSecondary]  = new Color(0.78f, 0.78f, 0.80f),
            [ThemeSlot.TextMuted]      = new Color(0.55f, 0.55f, 0.58f),
            [ThemeSlot.TextOnAccent]   = new Color(0.98f, 0.98f, 0.98f),
            [ThemeSlot.BorderDefault]  = new Color(0.25f, 0.25f, 0.28f, 1f),
            [ThemeSlot.BorderSubtle]   = new Color(0.18f, 0.18f, 0.20f, 1f),
            [ThemeSlot.StatusWarning]  = new Color(0.85f, 0.65f, 0.25f),
            [ThemeSlot.StatusDanger]   = new Color(0.80f, 0.30f, 0.30f),
            [ThemeSlot.StatusSuccess]  = new Color(0.35f, 0.70f, 0.40f),
        };
        Dictionary<FontRole, Font> fonts = new Dictionary<FontRole, Font>
        {
            [FontRole.Body]     = body,
            [FontRole.BodyBold] = bodyBold,
            [FontRole.Heading]  = heading,
            [FontRole.Display]  = display,
            [FontRole.Label]    = body,
            [FontRole.Caption]  = body,
            [FontRole.Mono]     = mono,
        };
        Dictionary<RadiusScale, float> radii = new Dictionary<RadiusScale, float>
        {
            [RadiusScale.None] = 0f,
            [RadiusScale.Sm]   = 4f,
            [RadiusScale.Md]   = 8f,
            [RadiusScale.Lg]   = 16f,
            [RadiusScale.Full] = 9999f,
        };
        Dictionary<ElevationScale, float> elev = new Dictionary<ElevationScale, float>
        {
            [ElevationScale.Flat] = 0f,
            [ElevationScale.Sm]   = 2f,
            [ElevationScale.Md]   = 6f,
            [ElevationScale.Lg]   = 12f,
        };
        return new Theme(colors, fonts, radii, elev);
    }
}
