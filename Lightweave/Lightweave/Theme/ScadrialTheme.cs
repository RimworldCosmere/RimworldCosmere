using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Theme;

public static class ScadrialTheme {
    public static Theme Build(Font body, Font bodyBold, Font heading, Font display, Font mono) {
        Dictionary<ThemeSlot, Color> colors = new Dictionary<ThemeSlot, Color> {
            [ThemeSlot.SurfacePrimary] = new Color(0.088f, 0.106f, 0.130f, 0.96f),
            [ThemeSlot.SurfaceRaised] = new Color(0.138f, 0.158f, 0.188f, 1.00f),
            [ThemeSlot.SurfaceSunken] = new Color(0.052f, 0.062f, 0.078f, 0.96f),
            [ThemeSlot.SurfaceAccent] = new Color(0.420f, 0.580f, 0.760f, 0.95f),
            [ThemeSlot.SurfaceShadow] = new Color(0.000f, 0.000f, 0.000f, 0.40f),
            [ThemeSlot.SurfaceInput] = new Color(0.170f, 0.195f, 0.230f, 1.00f),
            [ThemeSlot.SurfaceDisabled] = new Color(0.210f, 0.235f, 0.270f, 1.00f),
            [ThemeSlot.TextPrimary] = new Color(0.920f, 0.940f, 0.960f),
            [ThemeSlot.TextSecondary] = new Color(0.780f, 0.820f, 0.860f),
            [ThemeSlot.TextMuted] = new Color(0.560f, 0.600f, 0.660f),
            [ThemeSlot.TextOnAccent] = new Color(0.060f, 0.080f, 0.120f),
            [ThemeSlot.TextOnDanger] = new Color(0.985f, 0.980f, 0.985f),
            [ThemeSlot.BorderDefault] = new Color(0.380f, 0.430f, 0.500f, 1f),
            [ThemeSlot.BorderSubtle] = new Color(0.240f, 0.270f, 0.315f, 1f),
            [ThemeSlot.BorderFocus] = new Color(0.480f, 0.680f, 0.860f, 1f),
            [ThemeSlot.BorderHover] = new Color(0.540f, 0.590f, 0.660f, 1f),
            [ThemeSlot.BorderOff] = new Color(0.340f, 0.385f, 0.450f, 1f),
            [ThemeSlot.BorderDanger] = new Color(0.860f, 0.380f, 0.335f, 1f),
            [ThemeSlot.StatusWarning] = new Color(0.900f, 0.720f, 0.300f),
            [ThemeSlot.StatusDanger] = new Color(0.820f, 0.340f, 0.300f),
            [ThemeSlot.StatusSuccess] = new Color(0.500f, 0.720f, 0.460f),
            [ThemeSlot.InteractionHover] = new Color(0.920f, 0.940f, 0.960f, 1.00f),
            [ThemeSlot.InteractionPress] = new Color(0.000f, 0.000f, 0.000f, 1.00f),
        };
        return BaseTheme.Compose(colors, body, bodyBold, heading, display, mono);
    }
}