using Cosmere.Lightweave.Runtime;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Fonts;

[StaticConstructorOnStartup]
public static class FontLoader {
    static FontLoader() {
        Dictionary<string, Font> byName = LoadFontsFromBundles();
        LightweaveFonts.ArimoRegular = TryGet(byName, "Arimo-Regular");
        LightweaveFonts.ArimoBold = TryGet(byName, "Arimo-Bold");
        LightweaveFonts.CarlitoRegular = TryGet(byName, "Carlito-Regular");
        LightweaveFonts.CarlitoBold = TryGet(byName, "Carlito-Bold");
        LightweaveFonts.JetBrainsMono = TryGet(byName, "JetBrainsMono-Regular");
        LightweaveLog.Message($"fonts loaded: {byName.Count} from bundles.");
        GameFontOverride.Apply();
    }

    private static Dictionary<string, Font> LoadFontsFromBundles() {
        Dictionary<string, Font> result = new Dictionary<string, Font>();
        List<ModContentPack> mods = LoadedModManager.RunningModsListForReading;
        for (int i = 0; i < mods.Count; i++) {
            ModContentPack mod = mods[i];
            if (mod?.assetBundles?.loadedAssetBundles == null) {
                continue;
            }

            for (int b = 0; b < mod.assetBundles.loadedAssetBundles.Count; b++) {
                AssetBundle bundle = mod.assetBundles.loadedAssetBundles[b];
                if (bundle == null) {
                    continue;
                }

                Font[] fonts = bundle.LoadAllAssets<Font>();
                if (fonts == null) {
                    continue;
                }

                for (int f = 0; f < fonts.Length; f++) {
                    Font font = fonts[f];
                    if (font == null || string.IsNullOrEmpty(font.name)) {
                        continue;
                    }

                    result[font.name] = font;
                }
            }
        }

        return result;
    }

    private static Font? TryGet(Dictionary<string, Font> fonts, string name) {
        if (fonts.TryGetValue(name, out Font f)) {
            return f;
        }

        LightweaveLog.Warning($"font not found: {name}");
        return null;
    }
}