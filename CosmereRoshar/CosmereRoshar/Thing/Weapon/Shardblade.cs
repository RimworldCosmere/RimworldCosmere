using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Shader.Properties;
using Verse;

namespace Cosmere.Roshar.Thing.Weapon;

public abstract class Shardblade : ThingWithComps, IDynamicPalette {
    private CutoutLUT cutoutLUT => GetComp<CutoutLUT>();

    public abstract List<LUTPaletteMaterial> GetMaterials();

    public override void PostMake() {
        base.PostMake();
        cutoutLUT.palettes = GetMaterials();
    }

    public override void Notify_Equipped(Verse.Pawn pawn) {
        cutoutLUT.palettes = GetMaterials();
        base.Notify_Equipped(pawn);
    }
}