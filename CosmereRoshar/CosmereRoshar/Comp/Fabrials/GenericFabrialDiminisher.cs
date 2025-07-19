using System;
using System.Collections.Generic;
using System.Linq;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.ITabs;
using CosmereRoshar.Patches.Fabrials;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Comp.Fabrials;

public class BuildingFabrialBasicDiminisher : Building {
    public CompBasicFabrialDiminisher compBasicFabrialDiminisher;
    public CompFlickable compFlickerable;
    public CompGlower compGlower;


    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        compBasicFabrialDiminisher = GetComp<CompBasicFabrialDiminisher>();
        compFlickerable = GetComp<CompFlickable>();
        compGlower = GetComp<CompGlower>();
    }

    protected override void Tick() {
        compBasicFabrialDiminisher.CheckPower(compFlickerable.SwitchIsOn);
        ToggleGlow(compBasicFabrialDiminisher.powerOn);
        compBasicFabrialDiminisher.UsePower();
    }

    private void ToggleGlow(bool on) {
        if (Map != null) {
            if (on) {
                Stormlight? stormlightComp = compBasicFabrialDiminisher.insertedGemstone
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
        if (compBasicFabrialDiminisher.hasGemstone) {
            if (compBasicFabrialDiminisher.insertedGemstone.def == CosmereResources.ThingDefOf.CutRuby) {
                def.graphicData.attachments[0].Graphic.Print(layer, this, 0f);
            } else if (compBasicFabrialDiminisher.insertedGemstone.def == CosmereResources.ThingDefOf.CutDiamond) {
                def.graphicData.attachments[1].Graphic.Print(layer, this, 0f);
            } else if (compBasicFabrialDiminisher.insertedGemstone.def == CosmereResources.ThingDefOf.CutGarnet) {
                def.graphicData.attachments[2].Graphic.Print(layer, this, 0f);
            } else if (compBasicFabrialDiminisher.insertedGemstone.def == CosmereResources.ThingDefOf.CutEmerald) {
                def.graphicData.attachments[3].Graphic.Print(layer, this, 0f);
            } else if (compBasicFabrialDiminisher.insertedGemstone.def == CosmereResources.ThingDefOf.CutSapphire) {
                def.graphicData.attachments[4].Graphic.Print(layer, this, 0f);
            }
        }
    }
}

public class CompPropertiesBasicFabrialDiminisher : CompProperties {
    public CompPropertiesBasicFabrialDiminisher() {
        compClass = typeof(CompBasicFabrialDiminisher);
    }
}

public class CompBasicFabrialDiminisher : ThingComp, IGemstoneHandler, IFilterableComp {
    private List<ThingDef> filterListInt = [];
    public ThingWithComps? insertedGemstone;
    public bool powerOn;

    private List<GemSize> sizeFilterListInt = [
        GemSize.Chip,
        GemSize.Mark,
        GemSize.Broam,
    ];

    private float tempWhenTurnedOn;
    public new CompPropertiesBasicFabrialDiminisher props => (CompPropertiesBasicFabrialDiminisher)base.props;
    public CompGlower glowerComp => parent.GetComp<CompGlower>();
    public bool hasGemstone => insertedGemstone != null;

    public Spren currentSpren =>
        hasGemstone ? insertedGemstone.TryGetComp<CompCutGemstone>()?.capturedSpren ?? Spren.None : Spren.None;

    public List<ThingDef> filterList => filterListInt;

    public List<ThingDef> allowedSpheres { get; } = [
        CosmereResources.ThingDefOf.CutDiamond,
        CosmereResources.ThingDefOf.CutGarnet,
        CosmereResources.ThingDefOf.CutRuby,
        CosmereResources.ThingDefOf.CutSapphire,
        CosmereResources.ThingDefOf.CutEmerald,
    ];

    public List<GemSize> sizeFilterList => sizeFilterListInt;

    public void AddGemstone(ThingWithComps gemstone) {
        CompCutGemstone? gemstoneComp = gemstone.GetComp<CompCutGemstone>();
        if (gemstoneComp == null) return;
        insertedGemstone = gemstoneComp.parent;
        CultivationSprenPatch.RegisterBuilding((BuildingFabrialBasicDiminisher)parent);
    }

    public void RemoveGemstone() {
        if (insertedGemstone == null) return;
        Verse.Thing gemstoneToDrop = insertedGemstone;
        insertedGemstone = null;
        IntVec3 dropPosition = parent.Position;
        dropPosition.z -= 1;
        GenPlace.TryPlaceThing(gemstoneToDrop, dropPosition, parent.Map, ThingPlaceMode.Near);
        CultivationSprenPatch.UnregisterBuilding((BuildingFabrialBasicDiminisher)parent);
    }


    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);
        if (insertedGemstone != null) {
            CultivationSprenPatch.RegisterBuilding((BuildingFabrialBasicDiminisher)parent);
        }
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Deep.Look(ref insertedGemstone, "insertedGemstone");
        Scribe_Values.Look(ref powerOn, "PowerOn");
        Scribe_Values.Look(ref tempWhenTurnedOn, "TempWhenTurnedOn");
        Scribe_Collections.Look(ref sizeFilterListInt, "sizeFilterList", LookMode.Value);
        Scribe_Collections.Look(ref filterListInt, "filterList", LookMode.Def);
    }

    public void CheckPower(bool flickeredOn) {
        if (insertedGemstone?.TryGetComp(out Stormlight stormlight) ?? false) {
            bool power = stormlight.hasStormlight && flickeredOn;
            if (!powerOn && power) {
                tempWhenTurnedOn = parent.GetRoom().Temperature;
            }

            powerOn = power;
            return;
        }

        powerOn = false;
    }

    public void UsePower() {
        if (!powerOn || !(insertedGemstone?.TryGetComp(out Stormlight stormlight) ?? false)) {
            return;
        }

        stormlight.drainFactor = 2f;
        stormlight.CompTick();
        switch (insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren) {
            case Spren.Flame:
                DoFlameSprenPower();
                break;
            case Spren.Cold:
                DoColdSprenPower();
                break;
            case Spren.Pain:
                DoPainSprenPower();
                break;
            case Spren.Logic: //diamond
                DoLogicSprenPower();
                break;
            case Spren.Life: //emerald
                //Handled by patch
                break;
        }
    }


    private void DoFlameSprenPower() {
        if (!powerOn || parent.IsOutside()) return;
        int gemstoneSize = insertedGemstone.TryGetComp<CompCutGemstone>().gemstoneSize * 3; //3, 15, 60
        float currentTemp = parent.GetRoom().Temperature;
        if (currentTemp > tempWhenTurnedOn) {
            GenTemperature.PushHeat(parent.Position, parent.Map, 0f - gemstoneSize);
        }
    }

    private void DoColdSprenPower() {
        if (!powerOn || parent.IsOutside()) return;

        int gemstoneSize = insertedGemstone.TryGetComp<CompCutGemstone>().gemstoneSize * 3; //3, 15, 60
        float targetTemp = tempWhenTurnedOn;
        float currentTemp = parent.GetRoom().Temperature;
        if (currentTemp < targetTemp) {
            GenTemperature.PushHeat(parent.Position, parent.Map, gemstoneSize);
        }
    }

    private void DoPainSprenPower() {
        if (!powerOn) return;

        IntVec3 position = parent.Position;
        Map map = parent.Map;
        IEnumerable<IntVec3>? cells = GenRadial.RadialCellsAround(position, 5f, true);
        foreach (IntVec3 cell in cells) {
            Pawn pawn = cell.GetFirstPawn(map);
            if (pawn != null &&
                pawn.health.hediffSet.GetFirstHediffOfDef(CosmereRosharDefs.WhtwlPainrialDiminisher) == null &&
                pawn.Position.InHorDistOf(position, 5f)) {
                pawn.health.AddHediff(CosmereRosharDefs.WhtwlPainrialDiminisher);
            }
        }
    }

    private void DoLogicSprenPower() {
        if (!powerOn) return;
        IntVec3 position = parent.Position;
        Map map = parent.Map;
        IEnumerable<IntVec3>? cells = GenRadial.RadialCellsAround(position, 5f, true);
        foreach (IntVec3 cell in cells) {
            Pawn pawn = cell.GetFirstPawn(map);
            if (pawn != null &&
                pawn.health.hediffSet.GetFirstHediffOfDef(CosmereRosharDefs.WhtwlLogirialDiminisher) ==
                null &&
                pawn.Position.InHorDistOf(position, 5f)) {
                pawn.health.AddHediff(CosmereRosharDefs.WhtwlLogirialDiminisher);
            }
        }
    }

    public override string CompInspectStringExtra() {
        if (insertedGemstone == null) return "No gem in fabrial.";

        return "Spren: " +
               insertedGemstone.GetComp<CompCutGemstone>().capturedSpren +
               "\nStormlight: " +
               insertedGemstone.GetComp<Stormlight>().currentStormlight.ToString("F0") +
               "\ntime remaining: " +
               GetTimeRemaining();
    }

    private string GetTimeRemaining() {
        Stormlight? stormlightComp = insertedGemstone.TryGetComp<Stormlight>();
        float stormlightPerHour = 50.0f * stormlightComp.GetDrainRate(2f);
        if (Mathf.Approximately(stormlightPerHour, 0f)) {
            return "∞";
        }

        int hoursLeft = (int)(stormlightComp.currentStormlight / stormlightPerHour);
        int daysLeft = 0;
        if (hoursLeft > 24) {
            daysLeft = hoursLeft / 24;
        }

        hoursLeft %= 24;
        return daysLeft + "d " + hoursLeft + "h";
    }

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn) {
        Verse.Thing? cutGemstone = GenClosest.ClosestThing_Global(
            selPawn.Position,
            selPawn.Map.listerThings.AllThings.Where(thing =>
                StormlightUtilities.IsThingCutGemstone(thing) &&
                thing.TryGetComp<CompCutGemstone>().hasSprenInside &&
                thing.TryGetComp<Stormlight>().hasStormlight &&
                filterList.Contains(thing.def) &&
                sizeFilterList.Contains(thing.TryGetComp<CompCutGemstone>()?.GetGemSize() ?? GemSize.None)
            ),
            500f
        );

        //var cutGemstone = GenClosest.ClosestThing_Global(
        //       selPawn.Position,
        //       selPawn.Map.listerThings.AllThings.Where(
        //           thing => (StormlightUtilities.isThingCutGemstone(thing)) && (thing.TryGetComp<CompCutGemstone>().HasSprenInside && thing.TryGetComp<Stormlight>().HasStormlight)), 500f);

        Action? replaceGemAction = null;
        string replaceGemText = "No suitable gem available";
        if (cutGemstone != null) {
            replaceGemAction = () => {
                Job job = JobMaker.MakeJob(CosmereRosharDefs.WhtwlRefuelFabrial, parent, cutGemstone);
                if (job.TryMakePreToilReservations(selPawn, true)) {
                    selPawn.jobs.TryTakeOrderedJob(job);
                }
            };
            replaceGemText = $"Replace with {cutGemstone.Label}";
        }

        yield return new FloatMenuOption(replaceGemText, replaceGemAction);


        Action? removeGemAction = null;
        if (insertedGemstone != null) {
            removeGemAction = () => {
                Job job = JobMaker.MakeJob(CosmereRosharDefs.WhtwlRemoveFromFabrial, parent);
                if (job.TryMakePreToilReservations(selPawn, true)) {
                    selPawn.jobs.TryTakeOrderedJob(job);
                }
            };
        }

        yield return new FloatMenuOption("Remove Gemstone", removeGemAction);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra() {
        foreach (Gizmo gizmo in base.CompGetGizmosExtra()) {
            yield return gizmo;
        }

        yield return new Command_Action {
            defaultLabel = "Set Gem Filters",
            defaultDesc = "Click to choose which gems are allowed in this fabrial.",
            //icon = ContentFinder<Texture2D>.Get("UI/Icons/SomeIcon"), 
            icon = TexCommand.SelectShelf,
            action = () => { Find.WindowStack.Add(new DialogSphereFilter<CompBasicFabrialDiminisher>(this)); },
        };
    }
}