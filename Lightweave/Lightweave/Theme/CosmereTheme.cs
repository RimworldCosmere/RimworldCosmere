using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Theme;

public static class CosmereTheme {
    public static Theme Build(Font body, Font bodyBold, Font heading, Font display, Font mono) {
        Dictionary<ThemeSlot, Color> colors = new Dictionary<ThemeSlot, Color> {
            [ThemeSlot.SurfacePrimary] = new Color(0.085f, 0.072f, 0.058f, 0.96f),
            [ThemeSlot.SurfaceRaised] = new Color(0.140f, 0.118f, 0.098f, 1.00f),
            [ThemeSlot.SurfaceSunken] = new Color(0.048f, 0.040f, 0.032f, 0.96f),
            [ThemeSlot.SurfaceAccent] = new Color(0.831f, 0.659f, 0.341f, 1.00f),
            [ThemeSlot.SurfaceShadow] = new Color(0.000f, 0.000f, 0.000f, 0.40f),
            [ThemeSlot.SurfaceInput] = new Color(0.155f, 0.132f, 0.110f, 1.00f),
            [ThemeSlot.SurfaceDisabled] = new Color(0.195f, 0.170f, 0.140f, 1.00f),
            [ThemeSlot.TextPrimary] = new Color(0.940f, 0.910f, 0.840f),
            [ThemeSlot.TextSecondary] = new Color(0.780f, 0.745f, 0.680f),
            [ThemeSlot.TextMuted] = new Color(0.560f, 0.520f, 0.465f),
            [ThemeSlot.TextOnAccent] = new Color(0.080f, 0.040f, 0.020f),
            [ThemeSlot.TextOnDanger] = new Color(0.985f, 0.972f, 0.960f),
            [ThemeSlot.BorderDefault] = new Color(0.420f, 0.370f, 0.320f, 1f),
            [ThemeSlot.BorderSubtle] = new Color(0.250f, 0.220f, 0.190f, 1f),
            [ThemeSlot.BorderFocus] = new Color(0.900f, 0.720f, 0.400f, 1f),
            [ThemeSlot.BorderHover] = new Color(0.560f, 0.490f, 0.420f, 1f),
            [ThemeSlot.BorderOff] = new Color(0.380f, 0.335f, 0.290f, 1f),
            [ThemeSlot.BorderDanger] = new Color(0.850f, 0.340f, 0.270f, 1f),
            [ThemeSlot.StatusWarning] = new Color(0.940f, 0.700f, 0.280f),
            [ThemeSlot.StatusDanger] = new Color(0.800f, 0.280f, 0.220f),
            [ThemeSlot.StatusSuccess] = new Color(0.500f, 0.680f, 0.420f),
            [ThemeSlot.InteractionHover] = new Color(0.940f, 0.910f, 0.840f, 1.00f),
            [ThemeSlot.InteractionPress] = new Color(0.000f, 0.000f, 0.000f, 1.00f),
            [ThemeSlot.AccentMuted] = new Color(0.560f, 0.405f, 0.220f, 0.85f),
            [ThemeSlot.OverlayDim] = new Color(0.020f, 0.015f, 0.010f, 0.62f),
            [ThemeSlot.MapPreviewTint] = new Color(0.180f, 0.150f, 0.125f, 1.00f),
            [ThemeSlot.MetadataLabel] = new Color(0.420f, 0.385f, 0.340f),
        };
        return BaseTheme.Compose(colors, body, bodyBold, heading, display, mono);
    }
}