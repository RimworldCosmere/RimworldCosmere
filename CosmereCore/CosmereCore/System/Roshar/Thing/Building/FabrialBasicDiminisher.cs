using Cosmere.System.Roshar.Comp.Fabrials;
using Verse;

namespace Cosmere.System.Roshar.Thing.Building;

public class FabrialBasicDiminisher : FabrialBasicBuilding {
    private BasicFabrialDiminisher basicFabrialDiminisher = null!;

    protected override BasicFabrial FabrialComp => basicFabrialDiminisher;

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        basicFabrialDiminisher = GetComp<BasicFabrialDiminisher>();
    }


}
