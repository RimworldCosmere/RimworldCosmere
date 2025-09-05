using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Shader.Properties;
using Verse;

namespace Cosmere.Roshar.Thing.Weapon;

public abstract class Shardblade : ThingWithComps {
    private CutoutLUT cutoutLUT => GetComp<CutoutLUT>();

    public override void PostMake() {
        base.PostMake();
        cutoutLUT.palettes = GetBladeMaterials();
    }

    public override void Notify_Equipped(Verse.Pawn pawn) {
        cutoutLUT.palettes = GetBladeMaterials();
        base.Notify_Equipped(pawn);
    }

    protected abstract List<LUTPaletteMaterial> GetBladeMaterials();
}