using Cosmere.Core.Shader.Properties;

using UnityEngine;

namespace Cosmere.Core.Extension;

public static class MaterialPropertyBlockExtension {
    /// <summary>
    ///     Binds the palette through storage buffers, because a constant buffer pads every array element
    ///     to 16 bytes and three 32-entry arrays came to 1712 bytes against 512. The material carries them,
    ///     since a block-only binding never reaches this draw.
    /// </summary>
    public static void SetLUTColorMask(
        this MaterialPropertyBlock block,
        List<LUTPaletteMaterial> palette,
        Material material
    ) {
        Vector4[] colors = new Vector4[palette.Count];
        float[] metallic = new float[palette.Count];
        float[] smoothness = new float[palette.Count];

        for (int i = 0; i < palette.Count; i++) {
            colors[i] = palette[i].color;
            metallic[i] = palette[i].Metallic;
            smoothness[i] = palette[i].Smoothness;
        }

        block.SetVectorArray(CutoutAdvancedShaderProperties.PaletteColors, colors);
        block.SetFloatArray(CutoutAdvancedShaderProperties.MetallicValues, metallic);
        block.SetFloatArray(CutoutAdvancedShaderProperties.SmoothnessValues, smoothness);
        block.SetFloatArray(CutoutAdvancedShaderProperties.PaletteCount, new[] { (float)palette.Count });

        material.SetVectorArray(CutoutAdvancedShaderProperties.PaletteColors, colors);
        material.SetFloatArray(CutoutAdvancedShaderProperties.MetallicValues, metallic);
        material.SetFloatArray(CutoutAdvancedShaderProperties.SmoothnessValues, smoothness);
        material.SetFloatArray(CutoutAdvancedShaderProperties.PaletteCount, new[] { (float)palette.Count });
    }
}
