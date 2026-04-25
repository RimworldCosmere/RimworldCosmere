using Cosmere.Lightweave.Fonts;

namespace Cosmere.Lightweave.Theme;

public static class ThemeRegistry {
    private static Theme? cachedDefault;
    private static Theme? cachedCosmere;
    private static Theme? cachedScadrial;
    private static Theme? cachedRoshar;

    public static Theme Default {
        get {
            if (cachedDefault != null) {
                return cachedDefault;
            }

            cachedDefault = DefaultTheme.Build(
                LightweaveFonts.ArimoRegular,
                LightweaveFonts.ArimoBold,
                LightweaveFonts.ArimoBold,
                LightweaveFonts.CarlitoBold,
                LightweaveFonts.JetBrainsMono
            );
            return cachedDefault;
        }
    }

    public static Theme Cosmere {
        get {
            if (cachedCosmere != null) {
                return cachedCosmere;
            }

            cachedCosmere = CosmereTheme.Build(
                LightweaveFonts.ArimoRegular,
                LightweaveFonts.ArimoBold,
                LightweaveFonts.ArimoBold,
                LightweaveFonts.CarlitoBold,
                LightweaveFonts.JetBrainsMono
            );
            return cachedCosmere;
        }
    }

    public static Theme Scadrial {
        get {
            if (cachedScadrial != null) {
                return cachedScadrial;
            }

            cachedScadrial = ScadrialTheme.Build(
                LightweaveFonts.ArimoRegular,
                LightweaveFonts.ArimoBold,
                LightweaveFonts.ArimoBold,
                LightweaveFonts.CarlitoBold,
                LightweaveFonts.JetBrainsMono
            );
            return cachedScadrial;
        }
    }

    public static Theme Roshar {
        get {
            if (cachedRoshar != null) {
                return cachedRoshar;
            }

            cachedRoshar = RosharTheme.Build(
                LightweaveFonts.ArimoRegular,
                LightweaveFonts.ArimoBold,
                LightweaveFonts.ArimoBold,
                LightweaveFonts.CarlitoBold,
                LightweaveFonts.JetBrainsMono
            );
            return cachedRoshar;
        }
    }
}