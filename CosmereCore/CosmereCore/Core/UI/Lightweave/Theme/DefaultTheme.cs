using System.Collections.Generic;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Tokens;

namespace Cosmere.Core.UI.Lightweave.Theme;

public static class DefaultTheme
{
    public static Theme Build(Font body, Font bodyBold, Font heading, Font display, Font mono)
    {
        Dictionary<ThemeSlot, Color> colors = new Dictionary<ThemeSlot, Color>
        {
            [ThemeSlot.SurfacePrimary]  = new Color(0.14f, 0.14f, 0.14f, 0.95f),
            [ThemeSlot.SurfaceRaised]   = new Color(0.18f, 0.18f, 0.18f, 0.95f),
            [ThemeSlot.SurfaceSunken]   = new Color(0.09f, 0.09f, 0.09f, 0.95f),
            [ThemeSlot.SurfaceAccent]   = new Color(0.79f, 0.65f, 0.37f, 0.90f),
            [ThemeSlot.SurfaceInput]    = new Color(0.08f, 0.08f, 0.08f, 0.95f),
            [ThemeSlot.SurfaceDisabled] = new Color(0.14f, 0.14f, 0.14f, 0.60f),
            [ThemeSlot.TextPrimary]     = new Color(0.95f, 0.94f, 0.91f),
            [ThemeSlot.TextSecondary]   = new Color(0.84f, 0.82f, 0.77f),
            [ThemeSlot.TextMuted]       = new Color(0.72f, 0.70f, 0.65f),
            [ThemeSlot.TextOnAccent]    = new Color(0.12f, 0.10f, 0.07f),
            [ThemeSlot.BorderDefault]   = new Color(0.53f, 0.53f, 0.53f, 1f),
            [ThemeSlot.BorderSubtle]    = new Color(0.28f, 0.28f, 0.28f, 1f),
            [ThemeSlot.BorderFocus]     = new Color(1f, 0.92f, 0.55f, 1f),
            [ThemeSlot.BorderHover]     = new Color(0.70f, 0.70f, 0.70f, 1f),
            [ThemeSlot.StatusWarning]   = new Color(0.95f, 0.75f, 0.30f),
            [ThemeSlot.StatusDanger]    = new Color(0.78f, 0.28f, 0.28f),
            [ThemeSlot.StatusSuccess]   = new Color(0.55f, 0.75f, 0.35f),
        };
        return BaseTheme.Compose(colors, body, bodyBold, heading, display, mono);
    }
}
