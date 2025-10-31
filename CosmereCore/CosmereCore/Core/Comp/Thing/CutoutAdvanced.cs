using Cosmere.Core.Shader.Properties;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Comp.Thing;

/// <summary>
///     Blend modes for transitions between palette colors in CutoutAdvanced shader
/// </summary>
public enum CutoutAdvancedBlendMode : byte {
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
///     Component properties for CutoutAdvanced shader system.
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
public class CutoutAdvancedProperties : CompProperties {
    // ========================================
    // COLOR PALETTE SYSTEM
    // ========================================

    /// <summary>Blend mode for palette color transitions</summary>
    public CutoutAdvancedBlendMode blendMode = CutoutAdvancedBlendMode.Smooth;

    /// <summary>Strength of blending between palette colors (0-2)</summary>
    public float blendStrength = 0.5f;


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

    public CutoutAdvancedProperties() {
        compClass = typeof(CutoutAdvanced);
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
///     Types of mask textures used by the CutoutAdvanced system
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
internal class Mask(MaskType type, string path) {
    /// <summary>Whether the texture has been loaded yet</summary>
    public bool loaded;

    private Texture2D? maskInt;

    /// <summary>
    ///     The loaded mask texture. Loads automatically on first access.
    ///     Naming convention: [path]_m[type].png (e.g., "sword_mwear.png")
    /// </summary>
    public Texture2D? mask {
        get {
            //if (loaded) return maskInt;
            maskInt = ContentFinder<Texture2D>.Get($"{path}_m{type.ToString().ToLower()}", false);
            if (type == MaskType.Color && maskInt == null) {
                maskInt = ContentFinder<Texture2D>.Get($"{path}_m", false);
            }

            if (maskInt != null) {
                maskInt.ignoreMipmapLimit = true;
                maskInt.requestedMipmapLevel = 0;
            }

            loaded = true;

            return maskInt;
        }
    }
}

[StaticConstructorOnStartup]
public class CutoutAdvanced : ThingComp {
    public static readonly MaterialPropertyBlock MPB = new MaterialPropertyBlock();

    private readonly Dictionary<(MaskType, string), Mask?> maskCache = [];

    public float currentGlowLevel = 0;
    public float currentWearLevel = 0;

    public List<LUTPaletteMaterial> palettes = [];

    public new CutoutAdvancedProperties props => (CutoutAdvancedProperties)base.props;

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
        block.SetTexture(CutoutAdvancedShaderProperties.MainTex, material.mainTexture);
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

        SetShaderKeywords(material);

        //block.SetFloat(CutoutAdvancedShaderProperties.BlendMode, (byte)props.blendMode);
        //block.SetFloat(CutoutAdvancedShaderProperties.BlendStrength, props.blendStrength);
        block.SetFloat(CutoutAdvancedShaderProperties.BlendStrength, props.blendStrength);

        material.SetTexture(CutoutAdvancedShaderProperties.ColorMaskTex, colorMask!.mask);
        block.SetTexture(CutoutAdvancedShaderProperties.ColorMaskTex, colorMask!.mask);
        block.SetLUTColorMask(palettes);
        block.SetFloat(CutoutAdvancedShaderProperties.MaterialIntensity, props.materialIntesity);

        block.SetFloat(CutoutAdvancedShaderProperties.UseWearMask, props.useWear ? 1 : 0);
        if (props.useWear) {
            block.SetFloat(CutoutAdvancedShaderProperties.WearDarkness, props.wearDarkness);
            block.SetFloat(CutoutAdvancedShaderProperties.CurrentWearLevel, currentWearLevel);
            block.SetFloat(CutoutAdvancedShaderProperties.WearLevelCount, props.wearLevelCount);
            if (wearMask?.mask != null) {
                block.SetTexture(CutoutAdvancedShaderProperties.WearMaskTex, wearMask.mask);
            }
        }

        block.SetFloat(CutoutAdvancedShaderProperties.UseGlowMask, props.useGlow ? 1 : 0);
        if (props.useGlow) {
            block.SetColor(CutoutAdvancedShaderProperties.GlowColor, props.glowColor);
            block.SetFloat(CutoutAdvancedShaderProperties.GlowIntensity, props.glowIntensity);
            block.SetFloat(CutoutAdvancedShaderProperties.CurrentGlowLevel, currentGlowLevel);
            block.SetFloat(CutoutAdvancedShaderProperties.GlowLevelCount, props.glowLevelCount);
            if (glowMask?.mask != null) {
                block.SetTexture(CutoutAdvancedShaderProperties.GlowMaskTex, glowMask.mask);
            }
        }

        block.SetFloat(CutoutAdvancedShaderProperties.UseSpecialMask, props.useSpecial ? 1 : 0);
        if (props.useSpecial && specialMask?.mask != null) {
            block.SetTexture(CutoutAdvancedShaderProperties.SpecialMaskTex, specialMask.mask);
        }


        // Set pre-computed values for performance optimization
        block.SetVector(
            CutoutAdvancedShaderProperties.HighlightParams,
            new Vector4(
                8.0f, // x: base highlight size (increased to reduce banding artifacts)
                4.0f, // y: highlight power
                0.1f, // z: highlight intensity (reduced to compensate for larger size)
                0.0f // w: unused
            )
        );

        block.SetVector(CutoutAdvancedShaderProperties.RimLightCenter, new Vector2(0.5f, 0.5f));

        return block;
    }

    public override void CompDrawWornExtras() {
        base.CompDrawWornExtras();
        if (lastMaterial != null && lastGraphic != null) UpdateMaterialPropertyBlock(MPB, lastGraphic, lastMaterial);
    }

    private void SetShaderKeywords(Material material) {
        // Set blend mode keywords
        material.DisableKeyword("BLEND_LINEAR");
        material.DisableKeyword("BLEND_SMOOTH");
        material.DisableKeyword("BLEND_SHARP");
        material.DisableKeyword("BLEND_STEP");

        switch (props.blendMode) {
            case CutoutAdvancedBlendMode.Linear:
                material.EnableKeyword("BLEND_LINEAR");
                break;
            case CutoutAdvancedBlendMode.Smooth:
                material.EnableKeyword("BLEND_SMOOTH");
                break;
            case CutoutAdvancedBlendMode.Sharp:
                material.EnableKeyword("BLEND_SHARP");
                break;
            case CutoutAdvancedBlendMode.Step:
                material.EnableKeyword("BLEND_STEP");
                break;
        }

        // Set mask feature keywords
        if (props.useWear) {
            material.EnableKeyword("USE_WEAR_MASK");
        } else {
            material.DisableKeyword("USE_WEAR_MASK");
        }

        if (props.useGlow) {
            material.EnableKeyword("USE_GLOW_MASK");
        } else {
            material.DisableKeyword("USE_GLOW_MASK");
        }

        if (props.useSpecial) {
            material.EnableKeyword("USE_SPECIAL_MASK");
        } else {
            material.DisableKeyword("USE_SPECIAL_MASK");
        }
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Collections.Look(ref palettes, "palettes", LookMode.Deep);
    }
}