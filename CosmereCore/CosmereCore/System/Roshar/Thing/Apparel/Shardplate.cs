using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Shader.Properties;
using UnityEngine;

namespace Cosmere.System.Roshar.Thing.Apparel;

public abstract class Shardplate : RimWorld.Apparel, IDynamicPalette {
    protected CutoutAdvanced CutoutLUT => GetComp<CutoutAdvanced>();

    public abstract List<LUTPaletteMaterial> GetMaterials();

    public override void PostMake() {
        base.PostMake();
        CutoutLUT.palettes = GetMaterials();
    }

    public override void Notify_Equipped(Verse.Pawn pawn) {
        CutoutLUT.palettes = GetMaterials();
        base.Notify_Equipped(pawn);
    }

    public override void DrawWornExtras() {
        base.DrawWornExtras();
        CutoutLUT.palettes = GetMaterials();
        CutoutLUT.currentGlowLevel = CutoutLUT.currentWearLevel = Mathf.Clamp01(1f - HitPoints / (float)MaxHitPoints);
    }
}
