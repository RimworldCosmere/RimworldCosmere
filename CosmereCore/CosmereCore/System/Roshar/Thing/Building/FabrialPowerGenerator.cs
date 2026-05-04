using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Thing.Building;

public class FabrialPowerGenerator : FabrialBuilding {
    private CompPowerPlant compPowerPlant = null!;
    private Comp.Fabrials.FabrialPowerGenerator fabrialPowerGenerator = null!;

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        fabrialPowerGenerator = GetComp<Comp.Fabrials.FabrialPowerGenerator>();
        compPowerPlant = GetComp<CompPowerPlant>();
    }

    protected override void Tick() {
        fabrialPowerGenerator.UpdatePowerState(compFlickerable.SwitchIsOn);
        compPowerPlant.PowerOutput = fabrialPowerGenerator.powerOn ? 1000f : 0f;
        ToggleGlow(fabrialPowerGenerator.powerOn);
        fabrialPowerGenerator.UsePower();
    }
}
