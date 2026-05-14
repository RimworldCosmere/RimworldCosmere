using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Theme;

public static class DefaultTheme {
    public static Theme Build(Font body, Font bodyBold, Font heading, Font display, Font mono) {
        Dictionary<ThemeSlot, Color> colors = new Dictionary<ThemeSlot, Color> {
            [ThemeSlot.SurfacePrimary] = new Color(0.133f, 0.131f, 0.125f, 0.95f),
            [ThemeSlot.SurfaceRaised] = new Color(0.175f, 0.168f, 0.155f, 1.00f),
            [ThemeSlot.SurfaceSunken] = new Color(0.083f, 0.080f, 0.074f, 0.95f),
            [ThemeSlot.SurfaceTranslucent] = new Color(0.030f, 0.030f, 0.025f, 0.35f),
            [ThemeSlot.SurfaceAccent] = new Color(0.800f, 0.660f, 0.350f, 0.92f),
            [ThemeSlot.SurfaceShadow] = new Color(0.000f, 0.000f, 0.000f, 0.35f),
            [ThemeSlot.SurfaceInput] = new Color(0.165f, 0.160f, 0.148f, 1.00f),
            [ThemeSlot.SurfaceDisabled] = new Color(0.220f, 0.215f, 0.200f, 1.00f),
            [ThemeSlot.TextPrimary] = new Color(0.940f, 0.930f, 0.880f),
            [ThemeSlot.TextSecondary] = new Color(0.800f, 0.780f, 0.720f),
            [ThemeSlot.TextMuted] = new Color(0.560f, 0.545f, 0.500f),
            [ThemeSlot.TextOnAccent] = new Color(0.120f, 0.090f, 0.040f),
            [ThemeSlot.TextOnDanger] = new Color(0.985f, 0.975f, 0.970f),
            [ThemeSlot.BorderDefault] = new Color(0.165f, 0.140f, 0.115f, 1f),
            [ThemeSlot.BorderSubtle] = new Color(0.330f, 0.320f, 0.290f, 1f),
            [ThemeSlot.BorderFocus] = new Color(1.000f, 0.880f, 0.480f, 1f),
            [ThemeSlot.BorderHover] = new Color(0.380f, 0.300f, 0.190f, 1f),
            [ThemeSlot.BorderOff] = new Color(0.420f, 0.410f, 0.375f, 1f),
            [ThemeSlot.BorderDanger] = new Color(0.820f, 0.330f, 0.305f, 1f),
            [ThemeSlot.StatusWarning] = new Color(0.940f, 0.720f, 0.280f),
            [ThemeSlot.StatusDanger] = new Color(0.780f, 0.280f, 0.260f),
            [ThemeSlot.StatusSuccess] = new Color(0.550f, 0.720f, 0.360f),
            [ThemeSlot.InteractionHover] = new Color(1.000f, 1.000f, 1.000f, 1.00f),
            [ThemeSlot.InteractionPress] = new Color(0.000f, 0.000f, 0.000f, 1.00f),
            [ThemeSlot.AccentMuted] = new Color(0.580f, 0.480f, 0.300f, 0.85f),
            [ThemeSlot.OverlayDim] = new Color(0.030f, 0.030f, 0.025f, 0.62f),
            [ThemeSlot.MapPreviewTint] = new Color(0.205f, 0.198f, 0.180f, 1.00f),
            [ThemeSlot.MetadataLabel] = new Color(0.420f, 0.410f, 0.375f),
        };
        return BaseTheme.Compose(colors, body, bodyBold, heading, display, mono, BaseTheme.BuildFlatRadii());
    }
}