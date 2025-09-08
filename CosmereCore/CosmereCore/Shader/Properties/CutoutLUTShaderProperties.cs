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

/// <summary>
///     Shader property IDs for the CutoutLUT shader system.
///     Provides multi-texture masking for palette swapping, wear effects, glow effects, and special zones.
/// </summary>
public static class CutoutLUTShaderProperties {
    // ========================================
    // BASE TEXTURES
    // ========================================

    /// <summary>Main diffuse texture</summary>
    public static int MainTex = UnityEngine.Shader.PropertyToID("_MainTex");

    /// <summary>Grayscale mask for color palette selection (uses red channel)</summary>
    public static int ColorMaskTex = UnityEngine.Shader.PropertyToID("_ColorMaskTex");

    /// <summary>Grayscale mask for wear/damage zones (uses red channel)</summary>
    public static int WearMaskTex = UnityEngine.Shader.PropertyToID("_WearMaskTex");

    /// <summary>Grayscale mask for glow/emission zones (uses red channel)</summary>
    public static int GlowMaskTex = UnityEngine.Shader.PropertyToID("_GlowMaskTex");

    /// <summary>Grayscale mask for special effect zones (uses red channel)</summary>
    public static int SpecialMaskTex = UnityEngine.Shader.PropertyToID("_SpecialMaskTex");

    // ========================================
    // COLOR PALETTE SYSTEM
    // ========================================

    /// <summary>Strength of blending between palette colors (0-2)</summary>
    public static int BlendStrength = UnityEngine.Shader.PropertyToID("_BlendStrength");

    /// <summary>Number of colors in the palette array (max 32)</summary>
    public static int ColorCount = UnityEngine.Shader.PropertyToID("_ColorCount");

    /// <summary>Array of palette colors</summary>
    public static int Colors = UnityEngine.Shader.PropertyToID("_Colors");

    /// <summary>Fallback color when no mask is present</summary>
    public static int FallbackColor = UnityEngine.Shader.PropertyToID("_FallbackColor");

    /// <summary>Blend mode for palette transitions (0=None, 1=Linear, 2=Smooth, 3=Sharp, 4=Step)</summary>
    public static int BlendMode = UnityEngine.Shader.PropertyToID("_BlendMode");

    /// <summary>Intensity of metallic/smoothness effects (0-1)</summary>
    public static int MaterialIntensity = UnityEngine.Shader.PropertyToID("_MaterialIntensity");

    /// <summary>Array of metallic values for each palette color</summary>
    public static int MetallicValues = UnityEngine.Shader.PropertyToID("_MetallicValues");

    /// <summary>Array of smoothness values for each palette color</summary>
    public static int SmoothnessValues = UnityEngine.Shader.PropertyToID("_SmoothnessValues");

    // ========================================
    // MASK CONFIGURATION
    // ========================================

    /// <summary>Toggle to enable wear mask effects</summary>
    public static int UseWearMask = UnityEngine.Shader.PropertyToID("_UseWearMask");

    /// <summary>Toggle to enable glow mask effects</summary>
    public static int UseGlowMask = UnityEngine.Shader.PropertyToID("_UseGlowMask");

    /// <summary>Toggle to enable special zones mask effects</summary>
    public static int UseSpecialMask = UnityEngine.Shader.PropertyToID("_UseSpecialMask");

    /// <summary>Number of wear levels painted by artist (1-32)</summary>
    public static int WearLevelCount = UnityEngine.Shader.PropertyToID("_WearLevelCount");

    /// <summary>Number of glow levels painted by artist (1-32)</summary>
    public static int GlowLevelCount = UnityEngine.Shader.PropertyToID("_GlowLevelCount");

    /// <summary>Current wear state (0=pristine, 1=maximum wear)</summary>
    public static int CurrentWearLevel = UnityEngine.Shader.PropertyToID("_CurrentWearLevel");

    /// <summary>Current glow level (0=no glow, max=maximum glow)</summary>
    public static int CurrentGlowLevel = UnityEngine.Shader.PropertyToID("_CurrentGlowLevel");

    // ========================================
    // EFFECT PARAMETERS
    // ========================================

    /// <summary>How dark worn areas become (0=no change, 1=black)</summary>
    public static int WearDarkness = UnityEngine.Shader.PropertyToID("_WearDarkness");

    /// <summary>Color of glow emission</summary>
    public static int GlowColor = UnityEngine.Shader.PropertyToID("_GlowColor");

    /// <summary>Intensity multiplier for glow effects</summary>
    public static int GlowIntensity = UnityEngine.Shader.PropertyToID("_GlowIntensity");

    // ========================================
    // PRE-COMPUTED VALUES (OPTIMIZATION)
    // ========================================

    /// <summary>Pre-computed reciprocal of wear level count (1 / wearLevelCount)</summary>
    public static int WearZoneSize = UnityEngine.Shader.PropertyToID("_WearZoneSize");

    /// <summary>Pre-computed reciprocal of glow level count (1 / glowLevelCount)</summary>
    public static int GlowZoneSize = UnityEngine.Shader.PropertyToID("_GlowZoneSize");
}