using CosmereResources;
using CosmereResources.Extension;
using CosmereRoshar.Comp.Fabrials;
using CosmereRoshar.Comp.Thing;
using RimWorld;
using Verse;

namespace CosmereRoshar.Thing.Building;

public class FabrialBasicAugmenter : Verse.Building {
    public BasicFabrialAugmenter basicFabrialAugmenter;
    public CompFlickable compFlickerable;
    public CompGlower compGlower;


    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        basicFabrialAugmenter = GetComp<BasicFabrialAugmenter>();
        compFlickerable = GetComp<CompFlickable>();
        compGlower = GetComp<CompGlower>();
    }

    protected override void Tick() {
        basicFabrialAugmenter.CheckPower(compFlickerable.SwitchIsOn);
        ToggleGlow(basicFabrialAugmenter.powerOn);
        basicFabrialAugmenter.UsePower();
    }

    private void ToggleGlow(bool on) {
        if (Map == null) return;
        if (!on) {
            Map.glowGrid.DeRegisterGlower(compGlower);
            return;
        }

        if (!(basicFabrialAugmenter.insertedGemstone?.TryGetComp(out Stormlight stormlight) ?? false)) return;
        compGlower.GlowRadius = stormlight.maximumGlowRadius;
        if (stormlight.glowerComp != null) compGlower.GlowColor = stormlight.glowerComp.GlowColor;
        Map.glowGrid.RegisterGlower(compGlower);
    }

    public override void Print(SectionLayer layer) {
        base.Print(layer);
        if (basicFabrialAugmenter.insertedGemstone == null) return;

        if (basicFabrialAugmenter.insertedGemstone.IsCutGemOfType(GemDefOf.Ruby)) {
            def.graphicData.attachments[0].Graphic.Print(layer, this, 0f);
        } else if (basicFabrialAugmenter.insertedGemstone.IsCutGemOfType(GemDefOf.Diamond)) {
            def.graphicData.attachments[1].Graphic.Print(layer, this, 0f);
        } else if (basicFabrialAugmenter.insertedGemstone.IsCutGemOfType(GemDefOf.Garnet)) {
            def.graphicData.attachments[2].Graphic.Print(layer, this, 0f);
        } else if (basicFabrialAugmenter.insertedGemstone.IsCutGemOfType(GemDefOf.Emerald)) {
            def.graphicData.attachments[3].Graphic.Print(layer, this, 0f);
        } else if (basicFabrialAugmenter.insertedGemstone.IsCutGemOfType(GemDefOf.Sapphire)) {
            def.graphicData.attachments[4].Graphic.Print(layer, this, 0f);
        }
    }
}