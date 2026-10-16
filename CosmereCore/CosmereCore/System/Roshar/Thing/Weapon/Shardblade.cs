using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Shader.Properties;
using Verse;

namespace Cosmere.System.Roshar.Thing.Weapon;

public abstract class Shardblade : ThingWithComps, IDynamicPalette {
    private CutoutAdvanced CutoutLUT => GetComp<CutoutAdvanced>();

    public abstract List<LUTPaletteMaterial> GetMaterials();

    public override void PostMake() {
        base.PostMake();
        CutoutLUT.palettes = GetMaterials();
    }

    public override void Notify_Equipped(Verse.Pawn pawn) {
        CutoutLUT.palettes = GetMaterials();
        base.Notify_Equipped(pawn);
    }
}
