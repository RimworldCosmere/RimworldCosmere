using Cosmere.System.Roshar.Comp.Fabrials;
using Verse;

namespace Cosmere.System.Roshar.Thing.Building;

public class FabrialBasicAugmenter : FabrialBasicBuilding {
    private BasicFabrialAugmenter basicFabrialAugmenter = null!;

    protected override BasicFabrial FabrialComp => basicFabrialAugmenter;

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        basicFabrialAugmenter = GetComp<BasicFabrialAugmenter>();
    }


}
