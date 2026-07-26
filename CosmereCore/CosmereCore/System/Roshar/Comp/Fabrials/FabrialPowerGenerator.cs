using System;
using Cosmere.Core.Comp.Thing;
using Cosmere.System.Roshar.Dialog;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Comp.Fabrials;

public class FabrialPowerGenerator : ThingComp, IGemstoneHandler, IFilterableComp {
    private List<ThingDef> filterListInt = [];
    public ThingWithComps? insertedGemstone;
    public bool powerOn;

    public CompGlower? glowerComp => parent.TryGetComp<CompGlower>();
    public bool hasGemstone => insertedGemstone != null;
    public List<ThingDef> FilterList => filterListInt;

    public List<ThingDef> AllowedSpheres { get; } = [
        Core.ThingDefOf.CutGem,
        ThingDefOf.Cosmere_Roshar_Thing_Chip,
        ThingDefOf.Cosmere_Roshar_Thing_Mark,
        ThingDefOf.Cosmere_Roshar_Thing_Broam,
    ];

    public void AddGemstone(ThingWithComps gemstone) {
        if (gemstone.HasComp<InvestitureHolder>()) {
            insertedGemstone = gemstone;
        }
    }

    public void RemoveGemstone() {
        if (insertedGemstone == null) return;

        Verse.Thing gemstoneToDrop = insertedGemstone;
        insertedGemstone = null;
        IntVec3 dropPosition = parent.Position;
        dropPosition.z -= 1;
        GenPlace.TryPlaceThing(gemstoneToDrop, dropPosition, parent.Map, ThingPlaceMode.Near);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);
        if (filterListInt.Count == 0) {
            foreach (ThingDef def in AllowedSpheres) {
                if (def != null) FilterList.Add(def);
            }
        }
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Deep.Look(ref insertedGemstone, "insertedGemstone");
        Scribe_Values.Look(ref powerOn, "PowerOn");
        Scribe_Collections.Look(ref filterListInt, "FilterList", LookMode.Def);
    }

    public void UpdatePowerState(bool flickeredOn) {
        InvestitureHolder? investiture = insertedGemstone?.TryGetComp<InvestitureHolder>();
        if (investiture != null) {
            powerOn = investiture.currentInvestiture > 0 && flickeredOn;
            return;
        }

        powerOn = false;
    }

    public void UsePower() {
        InvestitureHolder? investiture = insertedGemstone?.TryGetComp<InvestitureHolder>();
        if (investiture == null) return;
        investiture.drainRate = powerOn ? 2.0f : 0.5f;
    }

    public override string CompInspectStringExtra() {
        if (insertedGemstone == null) return "No gem in fabrial.";

        InvestitureHolder? investiture = insertedGemstone.TryGetComp<InvestitureHolder>();
        return "Stormlight: " +
               (investiture?.currentInvestiture.ToString("F0") ?? "0") +
               "\ntime remaining: " +
               GetTimeRemaining();
    }

    private string GetTimeRemaining() {
        InvestitureHolder? investiture = insertedGemstone?.TryGetComp<InvestitureHolder>();
        if (investiture == null) return "∞";

        float tickRaresPerHour = (float)GenDate.TicksPerHour / GenTicks.TickRareInterval;
        float investiturePerHour = investiture.drainRate * tickRaresPerHour;
        if (Mathf.Approximately(investiturePerHour, 0f)) return "∞";

        int hoursLeft = (int)(investiture.currentInvestiture / investiturePerHour);
        int daysLeft = hoursLeft / 24;
        hoursLeft %= 24;
        return daysLeft + "d " + hoursLeft + "h";
    }

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn) {
        Verse.Thing? gemstone = GenClosest.ClosestThing_Global(
            selPawn.Position,
            selPawn.Map.listerThings.AllThings.Where(thing =>
                thing.HasComp<InvestitureHolder>() &&
                thing.TryGetComp<InvestitureHolder>().currentInvestiture > 0 &&
                FilterList.Contains(thing.def)
            ),
            500f
        );

        Action? replaceGemAction = null;
        string replaceGemText = "No suitable gem available";
        if (gemstone != null) {
            replaceGemAction = () => {
                Verse.AI.Job job = JobMaker.MakeJob(
                    JobDefOf.Cosmere_Roshar_RefuelFabrial,
                    parent,
                    gemstone
                );
                if (job.TryMakePreToilReservations(selPawn, true)) {
                    selPawn.jobs.TryTakeOrderedJob(job);
                }
            };
            replaceGemText = $"Replace with {gemstone.Label}";
        }

        yield return new FloatMenuOption(replaceGemText, replaceGemAction);

        Action? removeGemAction = null;
        if (insertedGemstone != null) {
            removeGemAction = () => {
                Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Cosmere_Roshar_RemoveFromFabrial, parent);
                if (job.TryMakePreToilReservations(selPawn, true)) {
                    selPawn.jobs.TryTakeOrderedJob(job);
                }
            };
        }

        yield return new FloatMenuOption("Remove Gemstone", removeGemAction);
    }

    public override IEnumerable<Verse.Gizmo> CompGetGizmosExtra() {
        foreach (Verse.Gizmo gizmo in base.CompGetGizmosExtra()) {
            yield return gizmo;
        }

        yield return new Command_Action {
            defaultLabel = "Set Gem Filters",
            defaultDesc = "Click to choose which gems are allowed in this fabrial.",
            icon = TexCommand.SelectShelf,
            action = () => { Find.WindowStack.Add(new SphereFilter<FabrialPowerGenerator>(this)); },
        };
    }
}