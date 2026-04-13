using UnityEngine;
using Verse;

namespace Cosmere.Core.Shader.Properties;

public interface IDynamicPalette {
    public List<LUTPaletteMaterial> GetMaterials();
}

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

public static class CutoutAdvancedShaderProperties {
    public static int MainTex = UnityEngine.Shader.PropertyToID("_MainTex");
    public static int ColorMaskTex = UnityEngine.Shader.PropertyToID("_ColorMaskTex");
    public static int WearMaskTex = UnityEngine.Shader.PropertyToID("_WearMaskTex");
    public static int GlowMaskTex = UnityEngine.Shader.PropertyToID("_GlowMaskTex");
    public static int SpecialMaskTex = UnityEngine.Shader.PropertyToID("_SpecialMaskTex");

    public static int BlendStrength = UnityEngine.Shader.PropertyToID("_BlendStrength");
    public static int ColorCount = UnityEngine.Shader.PropertyToID("_ColorCount");
    public static int Colors = UnityEngine.Shader.PropertyToID("_Colors");
    public static int MaterialIntensity = UnityEngine.Shader.PropertyToID("_MaterialIntensity");
    public static int MetallicValues = UnityEngine.Shader.PropertyToID("_MetallicValues");
    public static int SmoothnessValues = UnityEngine.Shader.PropertyToID("_SmoothnessValues");

    public static int UseWearMask = UnityEngine.Shader.PropertyToID("_UseWearMask");
    public static int UseGlowMask = UnityEngine.Shader.PropertyToID("_UseGlowMask");
    public static int UseSpecialMask = UnityEngine.Shader.PropertyToID("_UseSpecialMask");
    public static int WearLevelCount = UnityEngine.Shader.PropertyToID("_WearLevelCount");
    public static int GlowLevelCount = UnityEngine.Shader.PropertyToID("_GlowLevelCount");
    public static int CurrentWearLevel = UnityEngine.Shader.PropertyToID("_CurrentWearLevel");
    public static int CurrentGlowLevel = UnityEngine.Shader.PropertyToID("_CurrentGlowLevel");

    public static int WearDarkness = UnityEngine.Shader.PropertyToID("_WearDarkness");
    public static int GlowColor = UnityEngine.Shader.PropertyToID("_GlowColor");
    public static int GlowIntensity = UnityEngine.Shader.PropertyToID("_GlowIntensity");

    public static int HighlightParams = UnityEngine.Shader.PropertyToID("_HighlightParams");
    public static int RimLightCenter = UnityEngine.Shader.PropertyToID("_RimLightCenter");
}