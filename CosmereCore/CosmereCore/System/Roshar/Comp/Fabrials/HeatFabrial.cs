using System;
using Cosmere.Core;
using Cosmere.Core.Comp.Thing;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Comp.Fabrials;

public interface IGemstoneHandler {
    void RemoveGemstone();
    void AddGemstone(ThingWithComps gemstone);
}

public class BuildingHeatrialAdvanced : Building {
    public CompFlickable compFlickerable = null!;
    public CompGlower compGlower = null!;
    public CompHeatrial compHeatrial = null!;

    public override void SpawnSetup(Verse.Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        compHeatrial = GetComp<CompHeatrial>();
        compFlickerable = GetComp<CompFlickable>();
        compGlower = GetComp<CompGlower>();
    }

    public override void TickRare() {
        compHeatrial.CheckPower(compFlickerable.SwitchIsOn);
        if (compHeatrial.powerOn) {
            float ambientTemperature = AmbientTemperature;
            float num = ambientTemperature < 20f ? 1f :
                !(ambientTemperature > 120f) ? Mathf.InverseLerp(120f, 20f, ambientTemperature) : 0f;
            float num2 = GenTemperature.ControlTemperatureTempChange(Position, Map, 15f, 18f);
            bool flag = !Mathf.Approximately(num2, 0f);
            if (flag) {
                this.GetRoom().Temperature += num2;
            }
        }

        ToggleGlow(compHeatrial.powerOn);
        compHeatrial.UsePower();
    }

    private void ToggleGlow(bool on) {
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
    public Verse.Thing? insertedGemstone;
    public bool powerOn;
    public new CompPropertiesHeatrial props => (CompPropertiesHeatrial)base.props;
    public CompGlower glowerComp => parent.GetComp<CompGlower>();

    public void AddGemstone(ThingWithComps gemstone) {
        if (gemstone.IsCutGemOfType(GemDefOf.Ruby)) {
            insertedGemstone = gemstone;
        }
    }

    public void RemoveGemstone() {
        if (insertedGemstone != null) {
            Verse.Thing gemstoneToDrop = insertedGemstone;
            insertedGemstone = null!;
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

    public void CheckPower(bool flickeredOn) {
        if (insertedGemstone != null) {
            InvestitureHolder? investiture =
                (insertedGemstone as ThingWithComps)?.TryGetComp<InvestitureHolder>();
            if (investiture != null) {
                powerOn = investiture.currentInvestiture > 0 && flickeredOn;
                return;
            }
        }

        powerOn = false;
    }

    public void UsePower() {
        if (!powerOn || insertedGemstone == null) return;
        InvestitureHolder? investiture =
            (insertedGemstone as ThingWithComps)?.TryGetComp<InvestitureHolder>();
        if (investiture != null) {
            investiture.drainRate = 1.0f;
        }
    }

    public override string CompInspectStringExtra() {
        if (insertedGemstone == null) return "No gem in fabrial.";
        ThingWithComps? gemstone = insertedGemstone as ThingWithComps;
        InvestitureHolder? investiture =
            gemstone?.TryGetComp<InvestitureHolder>();
        return gemstone?.Label + "(" + (investiture?.currentInvestiture.ToString("F0") ?? "0") + ")";
    }

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn) {
        Verse.Thing? cutGemstone = GenClosest.ClosestThing_Global(
            selPawn.Position,
            selPawn.Map.listerThings.AllThings.Where(thing => thing.IsCutGemOfType(GemDefOf.Ruby)),
            500f
        );

        Action? replaceGemAction = null;
        string replaceGemText = "No suitable gem available";
        if (cutGemstone != null) {
            replaceGemAction = () => {
                Verse.AI.Job job = JobMaker.MakeJob(
                    JobDefOf.Cosmere_Roshar_RefuelFabrial,
                    parent,
                    cutGemstone
                );
                if (job.TryMakePreToilReservations(selPawn, true)) {
                    selPawn.jobs.TryTakeOrderedJob(job);
                }
            };
            replaceGemText = $"Replace with {cutGemstone.Label}";
        }

        yield return new FloatMenuOption(replaceGemText, replaceGemAction);
    }
}

public class CompPropertiesHeatrial : CompProperties {
    public CompPropertiesHeatrial() {
        compClass = typeof(CompHeatrial);
    }
}