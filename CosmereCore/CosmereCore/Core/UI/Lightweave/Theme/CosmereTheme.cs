using System.Collections.Generic;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Tokens;

namespace Cosmere.Core.UI.Lightweave.Theme;

public static class CosmereTheme
{
    public static Theme Build(Font body, Font bodyBold, Font heading, Font display, Font mono)
    {
        Dictionary<ThemeSlot, Color> colors = new Dictionary<ThemeSlot, Color>
        {
            [ThemeSlot.SurfacePrimary]  = new Color(0.10f, 0.11f, 0.12f, 0.96f),
            [ThemeSlot.SurfaceRaised]   = new Color(0.14f, 0.15f, 0.17f, 0.96f),
            [ThemeSlot.SurfaceSunken]   = new Color(0.06f, 0.07f, 0.08f, 0.96f),
            [ThemeSlot.SurfaceAccent]   = new Color(0.72f, 0.52f, 0.26f, 0.95f),
            [ThemeSlot.SurfaceInput]    = new Color(0.07f, 0.08f, 0.09f, 0.96f),
            [ThemeSlot.SurfaceDisabled] = new Color(0.12f, 0.13f, 0.14f, 0.60f),
            [ThemeSlot.TextPrimary]     = new Color(0.94f, 0.92f, 0.87f),
            [ThemeSlot.TextSecondary]   = new Color(0.80f, 0.80f, 0.82f),
            [ThemeSlot.TextMuted]       = new Color(0.66f, 0.68f, 0.72f),
            [ThemeSlot.TextOnAccent]    = new Color(0.08f, 0.06f, 0.04f),
            [ThemeSlot.BorderDefault]   = new Color(0.32f, 0.34f, 0.36f, 1f),
            [ThemeSlot.BorderSubtle]    = new Color(0.18f, 0.19f, 0.21f, 1f),
            [ThemeSlot.BorderFocus]     = new Color(0.86f, 0.68f, 0.38f, 1f),
            [ThemeSlot.BorderHover]     = new Color(0.42f, 0.44f, 0.48f, 1f),
            [ThemeSlot.StatusWarning]   = new Color(0.88f, 0.68f, 0.28f),
            [ThemeSlot.StatusDanger]    = new Color(0.76f, 0.28f, 0.22f),
            [ThemeSlot.StatusSuccess]   = new Color(0.48f, 0.68f, 0.42f),
        };
        return BaseTheme.Compose(colors, body, bodyBold, heading, display, mono);
    }
}
