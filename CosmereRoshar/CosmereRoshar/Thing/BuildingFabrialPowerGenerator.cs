using CosmereRoshar.Comp.Fabrials;
using CosmereRoshar.Comp.Thing;
using RimWorld;
using Verse;

namespace CosmereRoshar.Thing;

public class BuildingFabrialPowerGenerator : Building {
    public CompFlickable compFlickerable;
    public CompGlower compGlower;
    public CompPowerPlant compPowerPlant;
    public FabrialPowerGenerator fabrialPowerGenerator;


    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        fabrialPowerGenerator = GetComp<FabrialPowerGenerator>();
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