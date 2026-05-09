using System.IO;
using Cosmere.Lightweave.Runtime;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Fonts;

[StaticConstructorOnStartup]
public static class FontLoader {
    private const string LightweavePackageId = "cryptiklemur.lightweave";
    private const string FontsFolderName = "Fonts";

    static FontLoader() {
        string? fontsDir = ResolveFontsDirectory();
        if (fontsDir == null) {
            LightweaveLog.Warning("Lightweave mod content path not resolved; fonts will not load.");
            GameFontOverride.Apply();
            return;
        }

        LightweaveFonts.ArimoRegular = LoadTtf(fontsDir, "Arimo-Regular.ttf");
        LightweaveFonts.ArimoBold = LoadTtf(fontsDir, "Arimo-Bold.ttf");
        LightweaveFonts.CarlitoRegular = LoadTtf(fontsDir, "Carlito-Regular.ttf");
        LightweaveFonts.CarlitoBold = LoadTtf(fontsDir, "Carlito-Bold.ttf");
        LightweaveFonts.JetBrainsMono = LoadTtf(fontsDir, "JetBrainsMono-Regular.ttf");

        int loaded = 0;
        if (LightweaveFonts.ArimoRegular != null) loaded++;
        if (LightweaveFonts.ArimoBold != null) loaded++;
        if (LightweaveFonts.CarlitoRegular != null) loaded++;
        if (LightweaveFonts.CarlitoBold != null) loaded++;
        if (LightweaveFonts.JetBrainsMono != null) loaded++;
        LightweaveLog.Message($"fonts loaded: {loaded}/5 from {fontsDir}.");

        GameFontOverride.Apply();
    }

    private static string? ResolveFontsDirectory() {
        List<ModContentPack> mods = LoadedModManager.RunningModsListForReading;
        for (int i = 0; i < mods.Count; i++) {
            ModContentPack mod = mods[i];
            if (mod == null) {
                continue;
            }

            if (string.Equals(mod.PackageIdPlayerFacing, LightweavePackageId, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(mod.PackageId, LightweavePackageId, System.StringComparison.OrdinalIgnoreCase)) {
                return Path.Combine(mod.RootDir, FontsFolderName);
            }
        }

        return null;
    }

    private static Font? LoadTtf(string fontsDir, string filename) {
        string path = Path.Combine(fontsDir, filename);
        if (!File.Exists(path)) {
            LightweaveLog.Warning($"font file missing: {path}");
            return null;
        }

        Font? font = TryLoadFromPath(path);
        if (font == null) {
            LightweaveLog.Warning($"font load failed for {filename}; tried path constructor and OS fallback.");
            return null;
        }

        return font;
    }

    private static Font? TryLoadFromPath(string path) {
        string filename = Path.GetFileNameWithoutExtension(path);
        string[] osNames = BuildOsNameCandidates(filename);

        Font? osFont = Font.CreateDynamicFontFromOSFont(osNames, 16);
        if (osFont != null && osFont.dynamic) {
            return osFont;
        }

        // Path-based loads (Internal_CreateFontFromPath / Font(path) ctor) produce NON-dynamic
        // Font objects. Unity floods "Font size and style overrides are only supported for dynamic
        // fonts" on every Verse.Text call when those are wired into GUIStyle.font. So we only ship
        // a font here if the OS already has it registered. Bundled TTFs that arent on the host OS
        // get skipped, and consumers fall back to RimWorlds default fonts.
        LightweaveLog.Warning(
            $"font {filename}: no dynamic match on host OS (tried: {string.Join(", ", osNames)}). " +
            "Bundled TTFs require OS-side font registration to load as dynamic; skipping."
        );
        return null;
    }

    private static string[] BuildOsNameCandidates(string filename) {
        List<string> names = new List<string>();
        string spacedDash = filename.Replace('-', ' ');
        names.Add(spacedDash);

        int dashIdx = filename.IndexOf('-');
        string family = dashIdx > 0 ? filename.Substring(0, dashIdx) : filename;
        if (family != filename) {
            names.Add(family);
        }

        string camelSpaced = SplitCamelCase(family);
        if (camelSpaced != family) {
            if (dashIdx > 0) {
                names.Add(camelSpaced + " " + filename.Substring(dashIdx + 1));
            }
            names.Add(camelSpaced);
        }

        if (family.Equals("JetBrainsMono", System.StringComparison.OrdinalIgnoreCase)) {
            names.Add("JetBrainsMono Nerd Font");
            names.Add("JetBrains Mono Nerd Font");
            names.Add("JetBrainsMono NF");
        }

        return names.ToArray();
    }

    private static string SplitCamelCase(string input) {
        System.Text.StringBuilder sb = new System.Text.StringBuilder(input.Length + 4);
        for (int i = 0; i < input.Length; i++) {
            char c = input[i];
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(input[i - 1])) {
                sb.Append(' ');
            }

            sb.Append(c);
        }

        return sb.ToString();
    }
}
