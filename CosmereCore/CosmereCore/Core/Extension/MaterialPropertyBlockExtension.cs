using System;

using Cosmere.Core.Shader.Properties;

using UnityEngine;

namespace Cosmere.Core.Extension;

public static class MaterialPropertyBlockExtension {
    private const int PaletteSize = 32;

    /// A block texture does not reach the shader, so the palette goes on the material too or the
    /// lookup returns the transparent default and the alpha clip throws the whole sprite away.
    public static void SetLUTColorMask(
        this MaterialPropertyBlock block,
        List<LUTPaletteMaterial> palette,
        Material? material = null
    ) {
        Texture2D tex = PaletteTexture.For(palette);

        block.SetTexture(CutoutAdvancedShaderProperties.PaletteTex, tex);
        block.SetFloat(CutoutAdvancedShaderProperties.ColorCount, palette.Count);
        material?.SetTexture(CutoutAdvancedShaderProperties.PaletteTex, tex);
        material?.SetFloat(CutoutAdvancedShaderProperties.ColorCount, palette.Count);
    }

    /// <summary>
    ///     The palette as a texture rather than three shader arrays. A constant buffer pads every
    ///     array element to 16 bytes, so 32 colours plus 32 metallics plus 32 smoothnesses came to
    ///     1712 bytes against a 512 byte block and the shader refused to load.
    /// </summary>
    private static class PaletteTexture {
        private static readonly Dictionary<int, Texture2D> Cache = new Dictionary<int, Texture2D>();

        public static Texture2D For(List<LUTPaletteMaterial> palette) {
            int key = Key(palette);
            if (Cache.TryGetValue(key, out Texture2D? cached) && cached != null) return cached;

            // float, or 8 bits quantises metallic and smoothness. point, or entry 3 bleeds into 4.
            Texture2D tex = new Texture2D(PaletteSize, 2, TextureFormat.RGBAFloat, false) {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };

            for (int i = 0; i < PaletteSize; i++) {
                bool has = i < palette.Count;
                tex.SetPixel(i, 0, has ? palette[i].color : Color.clear);
                tex.SetPixel(
                    i, 1, has ? new Color(palette[i].metallic, palette[i].smoothness, 0f, 0f) : Color.clear
                );
            }

            tex.Apply(false, true);
            Cache[key] = tex;

            return tex;
        }

        private static int Key(List<LUTPaletteMaterial> palette) {
            HashCode hash = default;
            for (int i = 0; i < palette.Count && i < PaletteSize; i++) {
                hash.Add(palette[i].color);
                hash.Add(palette[i].metallic);
                hash.Add(palette[i].smoothness);
            }

            return hash.ToHashCode();
        }
    }
}
