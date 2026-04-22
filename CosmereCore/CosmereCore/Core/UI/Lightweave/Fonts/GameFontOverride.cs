using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Fonts;

[StaticConstructorOnStartup]
public static class GameFontOverride
{
    static GameFontOverride()
    {
        Font arimo = LightweaveFonts.ArimoRegular;
        if (arimo == null)
        {
            return;
        }
        for (int i = 0; i < Text.fontStyles.Length; i++)
        {
            GUIStyle style = Text.fontStyles[i];
            if (style != null)
            {
                style.font = arimo;
            }
        }
    }
}
