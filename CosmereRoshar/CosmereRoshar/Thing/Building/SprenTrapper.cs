using System.Collections.Generic;
using CosmereRoshar.Comp.Thing;
using UnityEngine;
using Verse;

namespace CosmereRoshar.Thing.Building;

public class SprenTrapper : Verse.Building {
    public CompGlower compGlower;
    public Dictionary<string, List<Spren>> gemstonePossibleSprenDict = new Dictionary<string, List<Spren>>();
    private SectionLayer layerTest;
    public Comp.Fabrials.SprenTrapper trapper;


    public override void Destroy(DestroyMode mode = DestroyMode.Vanish) {
        base.Destroy(mode);
        if (trapper != null) {
            trapper.RemoveGemstone();
        }
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        trapper = GetComp<Comp.Fabrials.SprenTrapper>();
        compGlower = GetComp<CompGlower>();
        gemstonePossibleSprenDict.Add(
            CosmereResources.ThingDefOf.CutDiamond.defName,
            [Spren.Logic, Spren.Light, Spren.Exhaustion]
        );
        gemstonePossibleSprenDict.Add(
            CosmereResources.ThingDefOf.CutGarnet.defName,
            [Spren.Rain, Spren.Exhaustion, Spren.Pain]
        );
        gemstonePossibleSprenDict.Add(
            CosmereResources.ThingDefOf.CutSapphire.defName,
            [Spren.Wind, Spren.Motion, Spren.Cold]
        );
        gemstonePossibleSprenDict.Add(
            CosmereResources.ThingDefOf.CutRuby.defName,
            [Spren.Flame, Spren.Anger]
        );
        gemstonePossibleSprenDict.Add(
            CosmereResources.ThingDefOf.CutEmerald.defName,
            [Spren.Flame, Spren.Life, Spren.Cultivation, Spren.Rain, Spren.Glory]
        );
    }

    public override void TickRare() {
        CaptureLoop();
        ToggleGlow();
        TriggerPrint();
        trapper.DrainStormlight();
    }

    private void ToggleGlow() {
        if (Map != null) {
            if (trapper.hasGemstone) {
                if (!trapper.sprenCaptured &&
                    !trapper.insertedGemstone.TryGetComp<Stormlight>().hasStormlight) {
                    compGlower.GlowColor = ColorInt.FromHdrColor(Color.red);
                } else if (trapper.sprenCaptured) {
                    compGlower.GlowColor = ColorInt.FromHdrColor(Color.green);
                } else {
                    compGlower.GlowColor = ColorInt.FromHdrColor(Color.blue);
                }

                Map.glowGrid.RegisterGlower(compGlower);
            } else {
                Map.glowGrid.DeRegisterGlower(compGlower);
            }
        }
    }

    private void CaptureLoop() {
        if (trapper.insertedGemstone != null && !trapper.sprenCaptured) {
            List<Spren> sprenList;
            gemstonePossibleSprenDict.TryGetValue(trapper.insertedGemstone.def.defName, out sprenList);
            foreach (Spren spren in sprenList) {
                trapper.TryCaptureSpren(spren);
                trapper.CheckTrapperState();
                if (trapper.sprenCaptured) break;
            }
        }

        trapper.CheckTrapperState();
    }

    //Add light system for trapped, empty, e.g.
    public override void Print(SectionLayer layer) {
        layerTest = layer;
        if (trapper.hasGemstone) {
            if (!trapper.sprenCaptured && !trapper.insertedGemstone.TryGetComp<Stormlight>().hasStormlight) {
                def.graphicData.attachments[0].Graphic.Print(layer, this, 0f);
            } else if (!trapper.sprenCaptured) {
                def.graphicData.attachments[1].Graphic.Print(layer, this, 0f);
            } else {
                def.graphicData.attachments[2].Graphic.Print(layer, this, 0f);
            }
        } else {
            base.Print(layer);
        }
    }

    public void TriggerPrint() {
        if (layerTest != null) {
            Print(layerTest);
        } else {
            Log.Message("Layer was null");
        }
    }
}