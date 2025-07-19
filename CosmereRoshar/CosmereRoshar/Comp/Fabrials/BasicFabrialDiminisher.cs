using System;
using System.Collections.Generic;
using System.Linq;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Dialog;
using CosmereRoshar.Patches.Fabrials;
using CosmereRoshar.Thing.Building;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereRoshar.Comp.Fabrials;

public class BasicFabrialDiminisher : ThingComp, IGemstoneHandler, IFilterableComp {
    private List<ThingDef> filterListInt = [];
    public ThingWithComps? insertedGemstone;
    public bool powerOn;

    private List<GemSize> sizeFilterListInt = [
        GemSize.Chip,
        GemSize.Mark,
        GemSize.Broam,
    ];

    private float tempWhenTurnedOn;
    public CompGlower glowerComp => parent.GetComp<CompGlower>();
    public bool hasGemstone => insertedGemstone != null;

    public Spren currentSpren =>
        hasGemstone ? insertedGemstone.TryGetComp<CompCutGemstone>()?.capturedSpren ?? Spren.None : Spren.None;

    public List<ThingDef> filterList => filterListInt;

    public List<ThingDef> allowedSpheres { get; } = [
        ThingDefOf.Cosmere_Roshar_Thing_Chip,
        ThingDefOf.Cosmere_Roshar_Thing_Mark,
        ThingDefOf.Cosmere_Roshar_Thing_Broam,
    ];

    public List<GemSize> sizeFilterList => sizeFilterListInt;

    public void AddGemstone(ThingWithComps gemstone) {
        CompCutGemstone? gemstoneComp = gemstone.GetComp<CompCutGemstone>();
        if (gemstoneComp == null) return;
        insertedGemstone = gemstoneComp.parent;
        CultivationSprenPatch.RegisterBuilding((FabrialBasicDiminisher)parent);
    }

    public void RemoveGemstone() {
        if (insertedGemstone == null) return;
        Verse.Thing gemstoneToDrop = insertedGemstone;
        insertedGemstone = null;
        IntVec3 dropPosition = parent.Position;
        dropPosition.z -= 1;
        GenPlace.TryPlaceThing(gemstoneToDrop, dropPosition, parent.Map, ThingPlaceMode.Near);
        CultivationSprenPatch.UnregisterBuilding((FabrialBasicDiminisher)parent);
    }


    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);
        if (insertedGemstone != null) {
            CultivationSprenPatch.RegisterBuilding((FabrialBasicDiminisher)parent);
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
                pawn.health.hediffSet.GetFirstHediffOfDef(CosmereRosharDefs.Cosmere_Roshar_PainrialDiminisher) ==
                null &&
                pawn.Position.InHorDistOf(position, 5f)) {
                pawn.health.AddHediff(CosmereRosharDefs.Cosmere_Roshar_PainrialDiminisher);
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
                pawn.health.hediffSet.GetFirstHediffOfDef(CosmereRosharDefs.Cosmere_Roshar_LogirialDiminisher) ==
                null &&
                pawn.Position.InHorDistOf(position, 5f)) {
                pawn.health.AddHediff(CosmereRosharDefs.Cosmere_Roshar_LogirialDiminisher);
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
                Verse.AI.Job job = JobMaker.MakeJob(
                    CosmereRosharDefs.Cosmere_Roshar_RefuelFabrial,
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


        Action? removeGemAction = null;
        if (insertedGemstone != null) {
            removeGemAction = () => {
                Verse.AI.Job job = JobMaker.MakeJob(CosmereRosharDefs.Cosmere_Roshar_RemoveFromFabrial, parent);
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
            action = () => { Find.WindowStack.Add(new SphereFilter<BasicFabrialDiminisher>(this)); },
        };
    }
}