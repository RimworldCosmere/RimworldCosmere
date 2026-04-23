using UnityEngine;
using Cosmere.Core.UI.Lightweave.Fonts;

namespace Cosmere.Core.UI.Lightweave.Theme;

public static class ThemeRegistry
{
    private static Theme? cachedDefault;
    private static Theme? cachedCosmere;

    public static Theme Default
    {
        get
        {
            if (cachedDefault != null)
            {
                return cachedDefault;
            }
            cachedDefault = DefaultTheme.Build(
                LightweaveFonts.ArimoRegular,
                LightweaveFonts.ArimoBold,
                LightweaveFonts.ArimoBold,
                LightweaveFonts.CarlitoBold,
                LightweaveFonts.JetBrainsMono);
            return cachedDefault;
        }
    }

    public static Theme Cosmere
    {
        get
        {
            if (cachedCosmere != null)
            {
                return cachedCosmere;
            }
            cachedCosmere = CosmereTheme.Build(
                LightweaveFonts.ArimoRegular,
                LightweaveFonts.ArimoBold,
                LightweaveFonts.ArimoBold,
                LightweaveFonts.CarlitoBold,
                LightweaveFonts.JetBrainsMono);
            return cachedCosmere;
        }
    }
}
