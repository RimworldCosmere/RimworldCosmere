using System.IO;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Runtime;

[StaticConstructorOnStartup]
internal static class LightweaveCursors {
    public static readonly Texture2D? Horizontal = LoadCursor("ResizeHorizontal");
    public static readonly Texture2D? Vertical = LoadCursor("ResizeVertical");
    public static readonly Texture2D? DiagonalNwSe = LoadCursor("ResizeDiagonalNwSe");
    public static readonly Texture2D? DiagonalNeSw = LoadCursor("ResizeDiagonalNeSw");
    public static readonly Texture2D? Move = LoadCursor("Move");

    public static readonly Vector2 Hotspot = new Vector2(16f, 16f);

    private static Texture2D? LoadCursor(string name) {
        foreach (ModContentPack mod in LoadedModManager.RunningMods) {
            string candidate = Path.Combine(
                mod.RootDir,
                "Assets",
                "Textures",
                "UI",
                "Cursors",
                name + ".png"
            );
            if (!File.Exists(candidate)) {
                continue;
            }

            try {
                byte[] bytes = File.ReadAllBytes(candidate);
                Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false, false);
                tex.LoadImage(bytes, false);
                tex.filterMode = FilterMode.Bilinear;
                tex.Apply(false, false);
                return tex;
            }
            catch (IOException ex) {
                LightweaveLog.Warning($"Failed to load cursor texture '{candidate}': {ex}");
            }
        }

        return null;
    }
}
