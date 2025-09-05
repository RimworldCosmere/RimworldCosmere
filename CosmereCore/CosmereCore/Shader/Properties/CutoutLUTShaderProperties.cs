using UnityEngine;
using Verse;

namespace Cosmere.Core.Shader.Properties;

public struct LUTPaletteMaterial : IExposable {
    public Vector4 color = default;

    private float metallicInt = 0;

    public float metallic {
        get => metallicInt;
        set => metallicInt = Mathf.Clamp01(value);
    }

    private float smoothnessInt = 0;

    public LUTPaletteMaterial() { }

    public float smoothness {
        get => smoothnessInt;
        set => smoothnessInt = Mathf.Clamp01(value);
    }

    public void ExposeData() {
        Scribe_Values.Look(ref color, "color");
        Scribe_Values.Look(ref metallicInt, "metallic");
        Scribe_Values.Look(ref smoothnessInt, "smoothness");
    }
}

public static class CutoutLUTShaderProperties {
    public static int MainTex = UnityEngine.Shader.PropertyToID("_MainTex");
    public static int LUTTex = UnityEngine.Shader.PropertyToID("_LUTTex");
    public static int BlendStrength = UnityEngine.Shader.PropertyToID("_BlendStrength");
    public static int ColorCount = UnityEngine.Shader.PropertyToID("_ColorCount");
    public static int Colors = UnityEngine.Shader.PropertyToID("_Colors");
    public static int FallbackColor = UnityEngine.Shader.PropertyToID("_FallbackColor");
    public static int BlendMode = UnityEngine.Shader.PropertyToID("_BlendMode");
    public static int MaterialIntensity = UnityEngine.Shader.PropertyToID("_MaterialIntensity");
    public static int MetallicValues = UnityEngine.Shader.PropertyToID("_MetallicValues");
    public static int SmoothnessValues = UnityEngine.Shader.PropertyToID("_SmoothnessValues");
    public static int UseGreenChannel = UnityEngine.Shader.PropertyToID("_UseGreenChannel");
    public static int UseBlueChannel = UnityEngine.Shader.PropertyToID("_UseBlueChannel");
    public static int UseAlphaChannel = UnityEngine.Shader.PropertyToID("_UseAlphaChannel");
    public static int GlowColor = UnityEngine.Shader.PropertyToID("_GlowColor");
    public static int GlowIntensity = UnityEngine.Shader.PropertyToID("_GlowIntensity");
    public static int WearDarkness = UnityEngine.Shader.PropertyToID("_WearDarkness");
}