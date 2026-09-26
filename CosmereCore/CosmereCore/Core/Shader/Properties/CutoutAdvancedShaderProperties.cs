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
    public static readonly int MainTex = UnityEngine.Shader.PropertyToID("_MainTex");
    public static readonly int ColorMaskTex = UnityEngine.Shader.PropertyToID("_ColorMaskTex");
    public static readonly int WearMaskTex = UnityEngine.Shader.PropertyToID("_WearMaskTex");
    public static readonly int GlowMaskTex = UnityEngine.Shader.PropertyToID("_GlowMaskTex");
    public static readonly int SpecialMaskTex = UnityEngine.Shader.PropertyToID("_SpecialMaskTex");
    public static readonly int PaletteTex = UnityEngine.Shader.PropertyToID("_PaletteTex");

    public static readonly int BlendStrength = UnityEngine.Shader.PropertyToID("_BlendStrength");
    public static readonly int ColorCount = UnityEngine.Shader.PropertyToID("_ColorCount");
    public static readonly int MaterialIntensity = UnityEngine.Shader.PropertyToID("_MaterialIntensity");

    public static readonly int UseWearMask = UnityEngine.Shader.PropertyToID("_UseWearMask");
    public static readonly int UseGlowMask = UnityEngine.Shader.PropertyToID("_UseGlowMask");
    public static readonly int UseSpecialMask = UnityEngine.Shader.PropertyToID("_UseSpecialMask");
    public static readonly int WearLevelCount = UnityEngine.Shader.PropertyToID("_WearLevelCount");
    public static readonly int GlowLevelCount = UnityEngine.Shader.PropertyToID("_GlowLevelCount");
    public static readonly int CurrentWearLevel = UnityEngine.Shader.PropertyToID("_CurrentWearLevel");
    public static readonly int CurrentGlowLevel = UnityEngine.Shader.PropertyToID("_CurrentGlowLevel");

    public static readonly int WearDarkness = UnityEngine.Shader.PropertyToID("_WearDarkness");
    public static readonly int GlowColor = UnityEngine.Shader.PropertyToID("_GlowColor");
    public static readonly int GlowIntensity = UnityEngine.Shader.PropertyToID("_GlowIntensity");

    public static readonly int HighlightParams = UnityEngine.Shader.PropertyToID("_HighlightParams");
    public static readonly int RimLightCenter = UnityEngine.Shader.PropertyToID("_RimLightCenter");
}
