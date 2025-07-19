using CosmereRoshar.Comp.Thing;
using RimWorld;
using Verse;

namespace CosmereRoshar.Thing.Building;

public class FabrialPowerGenerator : Verse.Building {
    public CompFlickable compFlickerable;
    public CompGlower compGlower;
    public CompPowerPlant compPowerPlant;
    public Comp.Fabrials.FabrialPowerGenerator fabrialPowerGenerator;


    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        fabrialPowerGenerator = GetComp<Comp.Fabrials.FabrialPowerGenerator>();
        compFlickerable = GetComp<CompFlickable>();
        compGlower = GetComp<CompGlower>();
        compPowerPlant = GetComp<CompPowerPlant>();
    }

    protected override void Tick() {
        fabrialPowerGenerator.CheckPower(compFlickerable.SwitchIsOn);
        if (!fabrialPowerGenerator.powerOn) {
            compPowerPlant.PowerOutput = 0f;
        } else {
            compPowerPlant.PowerOutput = 1000f;
        } //maybe add quality of stone affects power output later

        ToggleGlow(fabrialPowerGenerator.powerOn);
        fabrialPowerGenerator.UsePower();
    }

    private void ToggleGlow(bool on) {
        if (Map != null) {
            if (on) {
                Stormlight? stormlightComp = fabrialPowerGenerator.insertedGemstone
                    .GetComp<Stormlight>();
                compGlower.GlowRadius = stormlightComp.maximumGlowRadius;
                compGlower.GlowColor = stormlightComp.glowerComp.GlowColor;
                Map.glowGrid.RegisterGlower(compGlower);
            } else {
                Map.glowGrid.DeRegisterGlower(compGlower);
            }
        }
    }
}