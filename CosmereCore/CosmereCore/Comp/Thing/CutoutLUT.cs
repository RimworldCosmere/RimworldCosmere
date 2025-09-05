using Cosmere.Core.Shader.Properties;
using UnityEngine;
using Verse;

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
    public LUTPaletteMaterial[]? palettes;
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
    public readonly MaterialPropertyBlock mpb = new MaterialPropertyBlock();
    public LUTPaletteMaterial[]? palettes;
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

    public override void PostPostMake() {
        UpdateMaterialPropertyBlock();
    }

    public override void PostSpawnSetup(bool respawningAfterLoad) {
        UpdateMaterialPropertyBlock();
    }

    public override void Notify_Equipped(Pawn pawn) {
        UpdateMaterialPropertyBlock();
    }

    private void UpdateMaterialPropertyBlock() {
        mpb.Clear();
        if (palettes == null) {
            if (props.palettes == null) {
                return;
            }

            palettes = new LUTPaletteMaterial[32];
            for (int i = 0; i < props.palettes.Length; i++) {
                palettes[i] = props.palettes[i];
            }
        }

        mpb.SetFloat(CutoutLUTShaderProperties.BlendMode, (byte)props.blendMode);
        mpb.SetFloat(CutoutLUTShaderProperties.BlendStrength, props.blendStrength);

        mpb.SetLUTPalette(palettes);
        mpb.SetFloat(CutoutLUTShaderProperties.MaterialIntensity, props.materialIntesity);

        mpb.SetFloat(CutoutLUTShaderProperties.UseGreenChannel, props.useWearDamage ? 1 : 0);
        mpb.SetFloat(CutoutLUTShaderProperties.WearDarkness, props.wearDarkness);

        mpb.SetFloat(CutoutLUTShaderProperties.UseAlphaChannel, props.useMarkers ? 1 : 0);

        mpb.SetFloat(CutoutLUTShaderProperties.UseBlueChannel, props.useGlow ? 1 : 0);
        mpb.SetColor(CutoutLUTShaderProperties.GlowColor, props.glowColor);
        mpb.SetFloat(CutoutLUTShaderProperties.GlowIntensity, props.glowIntensity);

        mpb.SetColor(CutoutLUTShaderProperties.FallbackColor, props.fallbackColor);

        Texture2D? maskTex = ContentFinder<Texture2D>.Get(maskPath);
        if (maskTex != null) {
            mpb.SetTexture(CutoutLUTShaderProperties.LUTTex, maskTex);
        }
    }

    public override void PostExposeData() {
        base.PostExposeData();
        List<LUTPaletteMaterial>? palettesList = palettes?.ToList();
        Scribe_Collections.Look(ref palettesList, "palettes", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.LoadingVars && palettesList != null) {
            palettes = palettesList.ToArray();
        }

        if (Scribe.mode == LoadSaveMode.LoadingVars) {
            LongEventHandler.ExecuteWhenFinished(() => {
                    UpdateMaterialPropertyBlock();
                    if (parent.SpawnedParentOrMe is not Pawn pawn) return;
                    pawn.Drawer.renderer.EnsureGraphicsInitialized();
                    pawn.Drawer.renderer.SetAllGraphicsDirty();
                }
            );
        }
    }
}