using Cosmere.Core;
using Cosmere.System.Roshar.Comp.Fabrials;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Thing.Building;

public class FabrialBasicDiminisher : Verse.Building {
    public BasicFabrialDiminisher basicFabrialDiminisher = null!;
    public CompFlickable compFlickerable = null!;
    public CompGlower compGlower = null!;


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
        if (Map == null) return;
        if (on) {
            Map.glowGrid.RegisterGlower(compGlower);
        } else {
            Map.glowGrid.DeRegisterGlower(compGlower);
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
