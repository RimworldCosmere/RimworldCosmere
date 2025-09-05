using Cosmere.Core.Shader.Properties;
using UnityEngine;

namespace Cosmere.Core.Extension;

public static class MaterialPropertyBlockExtensions {
    public static void SetUnlitLUTPalette(this MaterialPropertyBlock block, Color[] colors) {
        Vector4[] colorArray = new Vector4[32];
        for (int i = 0; i < colors.Length && i < 32; i++) {
            colorArray[i] = colors[i];
        }

        block.SetVectorArray(CutoutLUTShaderProperties.Colors, colorArray);
        block.SetFloat(CutoutLUTShaderProperties.ColorCount, colors.Length);
    }

    public static void SetLUTPalette(this MaterialPropertyBlock block, LUTPaletteMaterial[] palette) {
        Vector4[] colors = new Vector4[32];
        float[] metallicValues = new float[32];
        float[] smoothnessValues = new float[32];

        for (int i = 0; i < palette.Length && i < 32; i++) {
            colors[i] = palette[i].color;
            metallicValues[i] = palette[i].metallic;
            smoothnessValues[i] = palette[i].smoothness;
        }

        block.SetVectorArray(CutoutLUTShaderProperties.Colors, colors);
        block.SetFloatArray(CutoutLUTShaderProperties.MetallicValues, metallicValues);
        block.SetFloatArray(CutoutLUTShaderProperties.SmoothnessValues, smoothnessValues);
        block.SetFloat(CutoutLUTShaderProperties.ColorCount, palette.Length);
    }
}