using Cosmere.Core.Shader.Properties;
using UnityEngine;
using Verse;
using Logger = Cosmere.Foundation.Logger;

namespace Cosmere.Core.Comp.Thing;

public enum CutoutLUTBlendMode : byte {
    None,
    Linear,
    Smooth,
    Sharp,
    Step,
}

public class CutoutLUTProperties : CompProperties {
    public CutoutLUTBlendMode blendMode = CutoutLUTBlendMode.Smooth;
    public float blendStrength = 0.5f;
    public Vector4 fallbackColor = Color.white;
    public Vector4 glowColor = Color.white;
    public float glowIntensity = 2f;
    public float materialIntesity = 1f;
    public List<LUTPaletteMaterial> palettes = [];
    public bool useGlow = false;
    public bool useMarkers = false;
    public bool useWearDamage = false;
    public float wearDarkness = 0.4f;

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

[StaticConstructorOnStartup]
public class CutoutLUT : ThingComp {
    public static readonly MaterialPropertyBlock MPB = new MaterialPropertyBlock();

    public List<LUTPaletteMaterial> palettes = [];
    private new CutoutLUTProperties props => (CutoutLUTProperties)base.props;

    private string maskPath {
        get {
            Graphic graphic = parent.Graphic;
            if (graphic is Graphic_RandomRotated rotated) {
                graphic = rotated.SubGraphic;
            }

            return graphic.maskPath.NullOrEmpty() ? graphic.path + Graphic_Single.MaskSuffix : graphic.maskPath;
        }
    }

    public MaterialPropertyBlock UpdateMaterialPropertyBlock() {
        MPB.Clear();
        if (palettes.Count == 0) {
            palettes = props.palettes;
        }

        if (palettes.NullOrEmpty()) {
            Logger.Error("palettes cannot be null or empty");
            return MPB;
        }

        MPB.SetFloat(CutoutLUTShaderProperties.BlendMode, (byte)props.blendMode);
        MPB.SetFloat(CutoutLUTShaderProperties.BlendStrength, props.blendStrength);

        MPB.SetLUTPalette(palettes);
        MPB.SetFloat(CutoutLUTShaderProperties.MaterialIntensity, props.materialIntesity);

        MPB.SetFloat(CutoutLUTShaderProperties.UseGreenChannel, props.useWearDamage ? 1 : 0);
        MPB.SetFloat(CutoutLUTShaderProperties.WearDarkness, props.wearDarkness);

        MPB.SetFloat(CutoutLUTShaderProperties.UseAlphaChannel, props.useMarkers ? 1 : 0);

        MPB.SetFloat(CutoutLUTShaderProperties.UseBlueChannel, props.useGlow ? 1 : 0);
        MPB.SetColor(CutoutLUTShaderProperties.GlowColor, props.glowColor);
        MPB.SetFloat(CutoutLUTShaderProperties.GlowIntensity, props.glowIntensity);

        MPB.SetColor(CutoutLUTShaderProperties.FallbackColor, props.fallbackColor);

        MPB.SetTexture(CutoutLUTShaderProperties.LUTTex, ContentFinder<Texture2D>.Get(maskPath, false));

        return MPB;
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Collections.Look(ref palettes, "palettes", LookMode.Deep);
    }
}