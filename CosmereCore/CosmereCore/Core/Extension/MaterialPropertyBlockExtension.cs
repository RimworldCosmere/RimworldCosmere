using Cosmere.Core.Shader.Properties;

using UnityEngine;
namespace Cosmere.Core.Extension;

public static class MaterialPropertyBlockExtension {
    public static void SetLUTColorMask(this MaterialPropertyBlock block, List<LUTPaletteMaterial> palette) {
        Vector4[] colors = new Vector4[32];
        float[] metallicValues = new float[32];
        float[] smoothnessValues = new float[32];

        for (int i = 0; i < palette.Count && i < 32; i++) {
            colors[i] = palette[i].color;
            metallicValues[i] = palette[i].metallic;
            smoothnessValues[i] = palette[i].smoothness;
        }

        block.SetVectorArray(CutoutAdvancedShaderProperties.Colors, colors);
        block.SetFloatArray(CutoutAdvancedShaderProperties.MetallicValues, metallicValues);
        block.SetFloatArray(CutoutAdvancedShaderProperties.SmoothnessValues, smoothnessValues);
        block.SetFloat(CutoutAdvancedShaderProperties.ColorCount, palette.Count);
    }
}