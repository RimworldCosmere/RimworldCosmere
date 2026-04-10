using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Thing.Building;

public class FabrialPowerGenerator : Verse.Building {
    public CompFlickable compFlickerable = null!;
    public CompGlower compGlower = null!;
    public CompPowerPlant compPowerPlant = null!;
    public Comp.Fabrials.FabrialPowerGenerator fabrialPowerGenerator = null!;


    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        fabrialPowerGenerator = GetComp<Comp.Fabrials.FabrialPowerGenerator>();
        compFlickerable = GetComp<CompFlickable>();
        compGlower = GetComp<CompGlower>();
        compPowerPlant = GetComp<CompPowerPlant>();
    }

    protected override void Tick() {
        fabrialPowerGenerator.CheckPower(compFlickerable.SwitchIsOn);
        compPowerPlant.PowerOutput = fabrialPowerGenerator.powerOn ? 1000f : 0f;
        ToggleGlow(fabrialPowerGenerator.powerOn);
        fabrialPowerGenerator.UsePower();
    }

    private void ToggleGlow(bool on) {
        if (Map == null) return;
        if (on) {
            Map.glowGrid.RegisterGlower(compGlower);
        } else {
            Map.glowGrid.DeRegisterGlower(compGlower);
        }
    }
}
