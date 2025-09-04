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
    public Color? fallbackColor;
    public float materialIntesity = 1f;
    public LUTPaletteMaterial[]? palettes;

    public CutoutLUTProperties() {
        compClass = typeof(CutoutLUT);
    }
}

[StaticConstructorOnStartup]
public class CutoutLUT : ThingComp {
    public readonly MaterialPropertyBlock mpb = new MaterialPropertyBlock();
    public LUTPaletteMaterial[]? palettes;
    private new CutoutLUTProperties props => (CutoutLUTProperties)base.props;

    public override void PostPostMake() {
        UpdateMaterialPropertyBlock();
    }

    public override void Notify_Equipped(Pawn pawn) {
        UpdateMaterialPropertyBlock();
    }

    public void UpdateMaterialPropertyBlock() {
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

        byte blendMode = (byte)props.blendMode;

        mpb.SetFloat(Shader.Properties.CutoutLUTProperties.BlendStrengthID, props.blendStrength);
        mpb.SetLUTPalette(palettes);
        mpb.SetFloat(Shader.Properties.CutoutLUTProperties.BlendModeID, blendMode);
        mpb.SetFloat(Shader.Properties.CutoutLUTProperties.MaterialIntensityID, props.materialIntesity);

        if (props.fallbackColor.HasValue) {
            mpb.SetColor(Shader.Properties.CutoutLUTProperties.FallbackColorID, props.fallbackColor.Value);
        }

        Texture2D? maskTex = ContentFinder<Texture2D>.Get(GetMaskPath(), false);
        if (maskTex != null) {
            mpb.SetTexture(Shader.Properties.CutoutLUTProperties.LUTTexID, maskTex);
        }
    }

    protected virtual string GetMaskPath() {
        Graphic graphic = parent.Graphic;
        if (graphic is Graphic_RandomRotated rotated) {
            graphic = rotated.SubGraphic;
        }

        return graphic.maskPath.NullOrEmpty() ? graphic.path + Graphic_Single.MaskSuffix : graphic.maskPath;
    }
}