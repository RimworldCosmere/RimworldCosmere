using UnityEngine;

namespace Cosmere.Core.Shader.Properties;

public readonly struct LUTPaletteMaterial(Color color, float metallic, float smoothness) {
    public readonly Color color = color;
    public readonly float metallic = Mathf.Clamp01(metallic);
    public readonly float smoothness = Mathf.Clamp01(smoothness);

    public LUTPaletteMaterial(Color color, double metallic, double smoothness) : this(
        color,
        (float)metallic,
        (float)smoothness
    ) { }

    public LUTPaletteMaterial(Color color, float metallic, double smoothness) : this(
        color,
        metallic,
        (float)smoothness
    ) { }

    public LUTPaletteMaterial(Color color, double metallic, float smoothness) : this(
        color,
        (float)metallic,
        smoothness
    ) { }
}

public static class CutoutLUTProperties {
    public static int MainTexID = UnityEngine.Shader.PropertyToID("_MainTex");
    public static int LUTTexID = UnityEngine.Shader.PropertyToID("_LUTTex");
    public static int BlendStrengthID = UnityEngine.Shader.PropertyToID("_BlendStrength");
    public static int ColorCountID = UnityEngine.Shader.PropertyToID("_ColorCount");
    public static int ColorsID = UnityEngine.Shader.PropertyToID("_Colors");
    public static int FallbackColorID = UnityEngine.Shader.PropertyToID("_FallbackColor");
    public static int BlendModeID = UnityEngine.Shader.PropertyToID("_BlendMode");
    public static int MaterialIntensityID = UnityEngine.Shader.PropertyToID("_MaterialIntensity");
    public static int MetallicValuesID = UnityEngine.Shader.PropertyToID("_MetallicValues");
    public static int SmoothnessValuesID = UnityEngine.Shader.PropertyToID("_SmoothnessValues");
}