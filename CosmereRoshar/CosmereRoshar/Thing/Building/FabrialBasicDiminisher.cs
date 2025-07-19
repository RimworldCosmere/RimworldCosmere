using CosmereResources;
using CosmereResources.Extension;
using CosmereRoshar.Comp.Fabrials;
using CosmereRoshar.Comp.Thing;
using RimWorld;
using Verse;

namespace CosmereRoshar.Thing.Building;

public class FabrialBasicDiminisher : Verse.Building {
    public BasicFabrialDiminisher basicFabrialDiminisher;
    public CompFlickable compFlickerable;
    public CompGlower compGlower;


    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        basicFabrialDiminisher = GetComp<BasicFabrialDiminisher>();
        compFlickerable = GetComp<CompFlickable>();
        compGlower = GetComp<CompGlower>();
    }

    protected override void Tick() {
        basicFabrialDiminisher.CheckPower(compFlickerable.SwitchIsOn);
        ToggleGlow(basicFabrialDiminisher.powerOn);
        basicFabrialDiminisher.UsePower();
    }

    private void ToggleGlow(bool on) {
        if (Map != null) {
            if (on) {
                Stormlight? stormlightComp = basicFabrialDiminisher.insertedGemstone
                    .GetComp<Stormlight>();
                compGlower.GlowRadius = stormlightComp.maximumGlowRadius;
                compGlower.GlowColor = stormlightComp.glowerComp.GlowColor;
                Map.glowGrid.RegisterGlower(compGlower);
            } else {
                Map.glowGrid.DeRegisterGlower(compGlower);
            }
        }
    }

    public override void Print(SectionLayer layer) {
        base.Print(layer);
        if (basicFabrialDiminisher.insertedGemstone == null) return;

        if (basicFabrialDiminisher.insertedGemstone.IsCutGemOfType(GemDefOf.Ruby)) {
            def.graphicData.attachments[0].Graphic.Print(layer, this, 0f);
        } else if (basicFabrialDiminisher.insertedGemstone.IsCutGemOfType(GemDefOf.Diamond)) {
            def.graphicData.attachments[1].Graphic.Print(layer, this, 0f);
        } else if (basicFabrialDiminisher.insertedGemstone.IsCutGemOfType(GemDefOf.Garnet)) {
            def.graphicData.attachments[2].Graphic.Print(layer, this, 0f);
        } else if (basicFabrialDiminisher.insertedGemstone.IsCutGemOfType(GemDefOf.Emerald)) {
            def.graphicData.attachments[3].Graphic.Print(layer, this, 0f);
        } else if (basicFabrialDiminisher.insertedGemstone.IsCutGemOfType(GemDefOf.Sapphire)) {
            def.graphicData.attachments[4].Graphic.Print(layer, this, 0f);
        }
    }
}