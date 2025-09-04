using Cosmere.Core.Shader.Properties;
using UnityEngine;

namespace Cosmere.Core.Extension;

public static class MaterialPropertyBlockExtensions {
    public static void SetUnlitLUTPalette(this MaterialPropertyBlock block, Color[] colors) {
        Vector4[] colorArray = new Vector4[32];
        for (int i = 0; i < colors.Length && i < 32; i++) {
            colorArray[i] = colors[i];
        }

        block.SetVectorArray(CutoutLUTProperties.ColorsID, colorArray);
        block.SetFloat(CutoutLUTProperties.ColorCountID, colors.Length);
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

        block.SetVectorArray(CutoutLUTProperties.ColorsID, colors);
        block.SetFloatArray(CutoutLUTProperties.MetallicValuesID, metallicValues);
        block.SetFloatArray(CutoutLUTProperties.SmoothnessValuesID, smoothnessValues);
        block.SetFloat(CutoutLUTProperties.ColorCountID, palette.Length);
    }
}