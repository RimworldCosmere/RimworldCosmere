using Cosmere.Core;
using Cosmere.System.Roshar.Comp.Fabrials;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Thing.Building;

public abstract class FabrialBasicBuilding : FabrialBuilding {
    protected abstract BasicFabrial FabrialComp { get; }

    protected override void Tick() {
        FabrialComp.UpdatePowerState(compFlickerable.SwitchIsOn);
        ToggleGlow(FabrialComp.powerOn);
        FabrialComp.UsePower();
    }

    public override void Print(SectionLayer layer) {
        base.Print(layer);
        if (FabrialComp.insertedGemstone == null) return;

        if (FabrialComp.insertedGemstone.IsCutGemOfType(GemDefOf.Ruby)) {
            def.graphicData.attachments[0].Graphic.Print(layer, this, 0f);
        } else if (FabrialComp.insertedGemstone.IsCutGemOfType(GemDefOf.Diamond)) {
            def.graphicData.attachments[1].Graphic.Print(layer, this, 0f);
        } else if (FabrialComp.insertedGemstone.IsCutGemOfType(GemDefOf.Garnet)) {
            def.graphicData.attachments[2].Graphic.Print(layer, this, 0f);
        } else if (FabrialComp.insertedGemstone.IsCutGemOfType(GemDefOf.Emerald)) {
            def.graphicData.attachments[3].Graphic.Print(layer, this, 0f);
        } else if (FabrialComp.insertedGemstone.IsCutGemOfType(GemDefOf.Sapphire)) {
            def.graphicData.attachments[4].Graphic.Print(layer, this, 0f);
        }
    }
}
