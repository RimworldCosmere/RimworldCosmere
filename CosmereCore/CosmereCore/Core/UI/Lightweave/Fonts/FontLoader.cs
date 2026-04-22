using UnityEngine;
using Verse;
using Cosmere.Core;

namespace Cosmere.Core.UI.Lightweave.Fonts;

[StaticConstructorOnStartup]
public static class FontLoader
{
    static FontLoader()
    {
        LightweaveFonts.ArimoRegular   = Load("Arimo-Regular");
        LightweaveFonts.ArimoBold      = Load("Arimo-Bold");
        LightweaveFonts.CarlitoRegular = Load("Carlito-Regular");
        LightweaveFonts.CarlitoBold    = Load("Carlito-Bold");
        LightweaveFonts.JetBrainsMono  = Load("JetBrainsMono-Regular");
        Logger.Info("Lightweave fonts loaded: Arimo / Carlito / JetBrainsMono.");
    }

    private static Font Load(string name)
    {
        Font f = ContentFinder<Font>.Get(name, reportFailure: false);
        if (f == null)
        {
            Logger.Warning($"Lightweave font not found: {name}");
        }
        return f!;
    }
}
