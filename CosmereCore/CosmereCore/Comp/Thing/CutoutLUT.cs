using Cosmere.Core.Shader.Properties;
using UnityEngine;
using Verse;
using Logger = Cosmere.Foundation.Logger;

namespace Cosmere.Core.Comp.Thing;

/// <summary>
///     Blend modes for transitions between palette colors in CutoutLUT shader
/// </summary>
public enum CutoutLUTBlendMode : byte {
    /// <summary>No blending - hard transitions</summary>
    None,

    /// <summary>Linear interpolation between colors</summary>
    Linear,

    /// <summary>Smooth curve interpolation (smoothstep)</summary>
    Smooth,

    /// <summary>Sharp curve interpolation (quadratic)</summary>
    Sharp,

    /// <summary>Hard step at 50% threshold</summary>
    Step,
}

/// <summary>
///     Component properties for CutoutLUT shader system.
///     Provides multi-texture masking for palette swapping, wear effects, glow effects, and special zones.
///     MASK TEXTURE NAMING CONVENTION:
///     - Color mask: [GraphicPath]_mcolor.png
///     - Wear mask: [GraphicPath]_mwear.png
///     - Glow mask: [GraphicPath]_mglow.png
///     - Special mask: [GraphicPath]_mspecial.png
///     LEVEL SYSTEM:
///     Artist paints grayscale masks divided into level ranges.
///     Example for 4 levels: 0-63, 64-127, 128-191, 192-255
/// </summary>
public class CutoutLUTProperties : CompProperties {
    // ========================================
    // COLOR PALETTE SYSTEM
    // ========================================

    /// <summary>Blend mode for palette color transitions</summary>
    public CutoutLUTBlendMode blendMode = CutoutLUTBlendMode.Smooth;

    /// <summary>Strength of blending between palette colors (0-2)</summary>
    public float blendStrength = 0.5f;

    /// <summary>Fallback color when no color mask is present</summary>
    public Color fallbackColor = Color.white;

    /// <summary>Color of glow emission</summary>
    public Color glowColor = Color.white;

    /// <summary>Intensity multiplier for glow effects (0-3)</summary>
    public float glowIntensity = 2f;

    /// <summary>Number of glow levels painted in the mask (1-32)</summary>
    public int glowLevelCount = 4;

    /// <summary>Intensity of metallic/smoothness material effects (0-1)</summary>
    public float materialIntesity = 1f;

    /// <summary>List of palette colors with metallic/smoothness values</summary>
    public List<LUTPaletteMaterial> palettes = [];

    // ========================================
    // GLOW SYSTEM  
    // ========================================

    /// <summary>Enable glow/emission effects</summary>
    public bool useGlow = false;

    // ========================================
    // SPECIAL ZONES SYSTEM
    // ========================================

    /// <summary>Enable special effect zones (gems, runes, etc.)</summary>
    public bool useSpecial = false;

    // ========================================
    // WEAR SYSTEM
    // ========================================

    /// <summary>Enable wear/damage effects</summary>
    public bool useWear = false;

    /// <summary>How dark worn areas become (0=no change, 1=black)</summary>
    public float wearDarkness = 0.4f;

    /// <summary>Number of wear levels painted in the mask (1-32)</summary>
    public int wearLevelCount = 4;

    public CutoutLUTProperties() {
        compClass = typeof(CutoutLUT);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef) {
        foreach (string configError in base.ConfigErrors(parentDef)) {
            yield return configError;
        }

        if (blendStrength > 2.0 || blendStrength < 0.0) {
            yield return "Blend Strength must be between 0.0 and 2.0 inclusive";
        }

        if (materialIntesity > 1.0 || materialIntesity < 0.0) {
            yield return "Material Intensity must be between 0.0 and 1.0 inclusive";
        }

        if (wearDarkness > 1.0 || wearDarkness < 0.0) {
            yield return "Wear Darkness must be between 0.0 and 1.0 inclusive";
        }

        if (glowIntensity > 3.0 || glowIntensity < 0.0) {
            yield return "Glow Intensity must be between 0.0 and 3.0 inclusive";
        }
    }
}

/// <summary>
///     Types of mask textures used by the CutoutLUT system
/// </summary>
public enum MaskType {
    /// <summary>Color palette selection mask</summary>
    Color,

    /// <summary>Wear/damage zones mask</summary>
    Wear,

    /// <summary>Glow/emission zones mask</summary>
    Glow,

    /// <summary>Special effect zones mask</summary>
    Special,
}

/// <summary>
///     Lazy-loading wrapper for mask textures.
///     Automatically loads textures using naming convention: [path]_m[type].png
/// </summary>
internal struct Mask(MaskType type, string path) {
    /// <summary>Whether the texture has been loaded yet</summary>
    public bool loaded;

    private Texture2D? maskInt;

    /// <summary>
    ///     The loaded mask texture. Loads automatically on first access.
    ///     Naming convention: [path]_m[type].png (e.g., "sword_mwear.png")
    /// </summary>
    public Texture2D? mask {
        get {
            if (loaded) return maskInt;
            maskInt = ContentFinder<Texture2D>.Get($"{path}_m{type.ToString().ToLower()}", false);
            if (type == MaskType.Color && maskInt == null) {
                maskInt = ContentFinder<Texture2D>.Get($"{path}_m", false);
            }

            loaded = true;

            return maskInt;
        }
    }
}

[StaticConstructorOnStartup]
public class CutoutLUT : ThingComp {
    public static readonly MaterialPropertyBlock MPB = new MaterialPropertyBlock();

    private readonly Dictionary<(MaskType, string), Mask?> maskCache = [];

    public float currentGlowLevel = 0;
    public float currentWearLevel = 0;

    public List<LUTPaletteMaterial> palettes = [];

    public new CutoutLUTProperties props => (CutoutLUTProperties)base.props;

    public Material? lastMaterial { get; set; }

    public Graphic? lastGraphic { get; set; }

    private Mask? GetMask(MaskType type, Graphic graphic, Material material) {
        switch (type) {
            case MaskType.Wear when !props.useWear:
            case MaskType.Glow when !props.useGlow:
            case MaskType.Special when !props.useSpecial:
                return null;
        }

        graphic = graphic is Graphic_RandomRotated rotated ? rotated.SubGraphic : graphic;
        string path = graphic is Graphic_Multi multi ? multi.GraphicPath : graphic.path;

        if (material != null) {
            int lastSlash = path.LastIndexOf('/');
            path = lastSlash >= 0 ? path.Substring(0, lastSlash + 1) + material.mainTexture.name : path;
        }

        if (maskCache.TryGetValue((type, path), out Mask? mask)) return mask;

        return maskCache[(type, path)] = new Mask(type, path);
    }

    public MaterialPropertyBlock UpdateMaterialPropertyBlock(
        MaterialPropertyBlock block,
        Graphic graphic,
        Material material
    ) {
        lastGraphic = graphic;
        lastMaterial = material;
        block.Clear();
        if (palettes.Count == 0) {
            palettes = props.palettes;
        }

        if (palettes.NullOrEmpty()) {
            Logger.Error("palettes cannot be null or empty");
            return block;
        }

        Mask? colorMask = GetMask(MaskType.Color, graphic, material);
        Mask? wearMask = GetMask(MaskType.Wear, graphic, material);
        Mask? glowMask = GetMask(MaskType.Glow, graphic, material);
        Mask? specialMask = GetMask(MaskType.Special, graphic, material);

        block.SetFloat(CutoutLUTShaderProperties.BlendMode, (byte)props.blendMode);
        block.SetFloat(CutoutLUTShaderProperties.BlendStrength, props.blendStrength);

        block.SetLUTColorMask(palettes);
        block.SetFloat(CutoutLUTShaderProperties.MaterialIntensity, props.materialIntesity);

        block.SetFloat(CutoutLUTShaderProperties.UseWearMask, props.useWear ? 1 : 0);
        if (props.useWear) {
            block.SetFloat(CutoutLUTShaderProperties.WearDarkness, props.wearDarkness);
            block.SetFloat(CutoutLUTShaderProperties.CurrentWearLevel, currentWearLevel);
            block.SetFloat(CutoutLUTShaderProperties.WearLevelCount, props.wearLevelCount);
            // Pre-compute reciprocal for optimization
            block.SetFloat(CutoutLUTShaderProperties.WearZoneSize, 1.0f / props.wearLevelCount);
            if (wearMask.HasValue && wearMask.Value.mask != null) {
                block.SetTexture(CutoutLUTShaderProperties.WearMaskTex, wearMask.Value.mask);
            }
        }

        block.SetFloat(CutoutLUTShaderProperties.UseGlowMask, props.useGlow ? 1 : 0);
        if (props.useGlow) {
            block.SetColor(CutoutLUTShaderProperties.GlowColor, props.glowColor);
            block.SetFloat(CutoutLUTShaderProperties.GlowIntensity, props.glowIntensity);
            block.SetFloat(CutoutLUTShaderProperties.CurrentGlowLevel, currentGlowLevel);
            block.SetFloat(CutoutLUTShaderProperties.GlowLevelCount, props.glowLevelCount);
            // Pre-compute reciprocal for optimization
            block.SetFloat(CutoutLUTShaderProperties.GlowZoneSize, 1.0f / props.glowLevelCount);
            if (glowMask.HasValue && glowMask.Value.mask != null) {
                block.SetTexture(CutoutLUTShaderProperties.GlowMaskTex, glowMask.Value.mask);
            }
        }

        block.SetFloat(CutoutLUTShaderProperties.UseSpecialMask, props.useSpecial ? 1 : 0);
        if (props.useSpecial && specialMask.HasValue && specialMask.Value.mask != null) {
            block.SetTexture(CutoutLUTShaderProperties.SpecialMaskTex, specialMask.Value.mask);
        }

        block.SetColor(CutoutLUTShaderProperties.FallbackColor, props.fallbackColor);

        if (colorMask!.Value.mask != null) {
            block.SetTexture(CutoutLUTShaderProperties.ColorMaskTex, colorMask.Value.mask);
        }

        return block;
    }

    public override void CompDrawWornExtras() {
        base.CompDrawWornExtras();
        if (lastMaterial != null && lastGraphic != null) UpdateMaterialPropertyBlock(MPB, lastGraphic, lastMaterial);
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Collections.Look(ref palettes, "palettes", LookMode.Deep);
    }
}