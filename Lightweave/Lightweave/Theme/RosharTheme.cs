using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Theme;

public static class RosharTheme {
    public static Theme Build(Font body, Font bodyBold, Font heading, Font display, Font mono) {
        Dictionary<ThemeSlot, Color> colors = new Dictionary<ThemeSlot, Color> {
            [ThemeSlot.SurfacePrimary] = new Color(0.940f, 0.935f, 0.905f, 0.97f),
            [ThemeSlot.SurfaceRaised] = new Color(0.980f, 0.975f, 0.945f, 1.00f),
            [ThemeSlot.SurfaceSunken] = new Color(0.880f, 0.870f, 0.840f, 0.97f),
            [ThemeSlot.SurfaceAccent] = new Color(0.175f, 0.470f, 0.515f, 1.00f),
            [ThemeSlot.SurfaceShadow] = new Color(0.090f, 0.105f, 0.130f, 0.20f),
            [ThemeSlot.SurfaceInput] = new Color(1.000f, 0.995f, 0.965f, 1.00f),
            [ThemeSlot.SurfaceDisabled] = new Color(0.860f, 0.855f, 0.825f, 1.00f),
            [ThemeSlot.TextPrimary] = new Color(0.140f, 0.155f, 0.180f),
            [ThemeSlot.TextSecondary] = new Color(0.320f, 0.340f, 0.375f),
            [ThemeSlot.TextMuted] = new Color(0.480f, 0.495f, 0.525f),
            [ThemeSlot.TextOnAccent] = new Color(0.985f, 0.985f, 0.970f),
            [ThemeSlot.TextOnDanger] = new Color(0.985f, 0.975f, 0.970f),
            [ThemeSlot.BorderDefault] = new Color(0.600f, 0.640f, 0.660f, 1f),
            [ThemeSlot.BorderSubtle] = new Color(0.770f, 0.790f, 0.790f, 1f),
            [ThemeSlot.BorderFocus] = new Color(0.280f, 0.680f, 0.740f, 1f),
            [ThemeSlot.BorderHover] = new Color(0.440f, 0.505f, 0.540f, 1f),
            [ThemeSlot.BorderOff] = new Color(0.560f, 0.600f, 0.620f, 1f),
            [ThemeSlot.BorderDanger] = new Color(0.720f, 0.250f, 0.230f, 1f),
            [ThemeSlot.StatusWarning] = new Color(0.720f, 0.540f, 0.180f),
            [ThemeSlot.StatusDanger] = new Color(0.680f, 0.220f, 0.200f),
            [ThemeSlot.StatusSuccess] = new Color(0.300f, 0.560f, 0.340f),
            [ThemeSlot.InteractionHover] = new Color(0.090f, 0.105f, 0.130f, 1.00f),
            [ThemeSlot.InteractionPress] = new Color(0.060f, 0.070f, 0.090f, 1.00f),
            [ThemeSlot.AccentMuted] = new Color(0.310f, 0.560f, 0.580f, 0.75f),
            [ThemeSlot.OverlayDim] = new Color(0.070f, 0.085f, 0.105f, 0.55f),
            [ThemeSlot.MapPreviewTint] = new Color(0.820f, 0.810f, 0.780f, 1.00f),
            [ThemeSlot.MetadataLabel] = new Color(0.600f, 0.615f, 0.640f),
        };
        return BaseTheme.Compose(colors, body, bodyBold, heading, display, mono);
    }
}