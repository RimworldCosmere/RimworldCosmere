using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Shader.Properties;
using UnityEngine;

namespace Cosmere.Roshar.Thing.Apparel;

public abstract class Shardplate : RimWorld.Apparel, IDynamicPalette {
    protected CutoutLUT cutoutLUT => GetComp<CutoutLUT>();

    public abstract List<LUTPaletteMaterial> GetMaterials();

    public override void PostMake() {
        base.PostMake();
        cutoutLUT.palettes = GetMaterials();
    }

    public override void Notify_Equipped(Verse.Pawn pawn) {
        cutoutLUT.palettes = GetMaterials();
        base.Notify_Equipped(pawn);
    }

    public override void DrawWornExtras() {
        base.DrawWornExtras();
        cutoutLUT.palettes = GetMaterials();
        cutoutLUT.currentGlowLevel = cutoutLUT.currentWearLevel = Mathf.Clamp01(1f - HitPoints / (float)MaxHitPoints);
    }
}