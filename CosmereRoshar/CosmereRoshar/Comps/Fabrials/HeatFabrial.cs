using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CosmereRoshar;

public interface IGemstoneHandler {
    void RemoveGemstone();
    void AddGemstone(ThingWithComps gemstone);
}

public class Building_Heatrial_Advanced : Building {
    public CompFlickable compFlickerable;
    public CompGlower compGlower;
    public CompHeatrial compHeatrial;

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        compHeatrial = GetComp<CompHeatrial>();
        compFlickerable = GetComp<CompFlickable>();
        compGlower = GetComp<CompGlower>();
    }

    public override void TickRare() {
        compHeatrial.checkPower(compFlickerable.SwitchIsOn);
        if (compHeatrial.PowerOn) {
            float ambientTemperature = AmbientTemperature;
            float num = ambientTemperature < 20f ? 1f :
                !(ambientTemperature > 120f) ? Mathf.InverseLerp(120f, 20f, ambientTemperature) : 0f;
            float num2 = GenTemperature.ControlTemperatureTempChange(Position, Map, 15f, 18f);
            bool flag = !Mathf.Approximately(num2, 0f);
            if (flag) {
                this.GetRoom().Temperature += num2;
            }
        }

        toggleGlow(compHeatrial.PowerOn);
        compHeatrial.usePower();
    }

    private void toggleGlow(bool on) {
        if (Map != null) {
            if (on) {
                Map.glowGrid.RegisterGlower(compGlower);
            } else {
                Map.glowGrid.DeRegisterGlower(compGlower);
            }
        }
    }
}

public class CompHeatrial : ThingComp, IGemstoneHandler {
    public Thing insertedGemstone;
    public bool PowerOn;
    public CompProperties_Heatrial Props => (CompProperties_Heatrial)props;
    public CompGlower GlowerComp => parent.GetComp<CompGlower>();

    public void AddGemstone(ThingWithComps gemstone) {
        CompCutGemstone? gemstoneComp = gemstone.GetComp<CompCutGemstone>();
        if (gemstoneComp != null && gemstoneComp.parent.def == CosmereResources.ThingDefOf.CutRuby) {
            insertedGemstone = gemstoneComp.parent;
        }
    }

    public void RemoveGemstone() {
        if (insertedGemstone != null) {
            Thing gemstoneToDrop = insertedGemstone;
            insertedGemstone = null;
            IntVec3 dropPosition = parent.Position;
            GenPlace.TryPlaceThing(gemstoneToDrop, dropPosition, parent.Map, ThingPlaceMode.Near);
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);
    }

    public override void PostExposeData() {
        base.PostExposeData();
    }

    public void checkPower(bool flickeredOn) {
        if (insertedGemstone != null) {
            CompStormlight? stormlightComp = (insertedGemstone as ThingWithComps).GetComp<CompStormlight>();
            if (stormlightComp != null) {
                PowerOn = stormlightComp.HasStormlight && flickeredOn;
                return;
            }
        }

        PowerOn = false;
    }

    public void usePower() {
        if (PowerOn &&
            insertedGemstone != null &&
            insertedGemstone.TryGetComp<CompStormlight>() is CompStormlight stormlightComp) {
            //stormlightComp.drainStormLight(7f);
        }
    }

    public override string CompInspectStringExtra() {
        string gemName = "No gem in fabrial.";

        if (insertedGemstone != null) {
            ThingWithComps gemstone = insertedGemstone as ThingWithComps;
            gemName = gemstone.GetComp<CompCutGemstone>().GetFullLabel +
                      "(" +
                      gemstone.GetComp<CompStormlight>().Stormlight.ToString("F0") +
                      ")";
        }

        return gemName;
    }

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn) {
        Thing? cutGemstone = GenClosest.ClosestThing_Global(
            selPawn.Position,
            selPawn.Map.listerThings.AllThings.Where(thing => thing.def == CosmereResources.ThingDefOf.CutRuby),
            500f
        );

        Action replaceGemAction = null;
        string replaceGemText = "No suitable gem available";
        if (cutGemstone != null) {
            replaceGemAction = () => {
                Job job = JobMaker.MakeJob(CosmereRosharDefs.whtwl_RefuelFabrial, parent, cutGemstone);
                if (job.TryMakePreToilReservations(selPawn, true)) {
                    selPawn.jobs.TryTakeOrderedJob(job);
                }
            };
            replaceGemText = $"Replace with {cutGemstone.Label}";
        }

        yield return new FloatMenuOption(replaceGemText, replaceGemAction);
    }
}

public class CompProperties_Heatrial : CompProperties {
    public CompProperties_Heatrial() {
        compClass = typeof(CompHeatrial);
    }
}