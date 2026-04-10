using System;
using Cosmere.Core.Comp.Thing;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using Cosmere.System.Roshar.Patch.Fabrials;
using Cosmere.System.Roshar.Thing.Building;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Comp.Fabrials;

public class BasicFabrialDiminisher : ThingComp, IGemstoneHandler, IFilterableComp {
    private List<ThingDef> filterListInt = [];
    public ThingWithComps? insertedGemstone;
    public bool powerOn;

    private float tempWhenTurnedOn;
    public CompGlower glowerComp => parent.GetComp<CompGlower>();
    public bool hasGemstone => insertedGemstone != null;

    public SprenType? currentSpren =>
        hasGemstone
            ? insertedGemstone.TryGetComp<SprenContainer>()?.CapturedSprenType
            : null;

    public List<ThingDef> filterList => filterListInt;

    public List<ThingDef> allowedSpheres { get; } = [
        Core.ThingDefOf.CutGem,
    ];

    public void AddGemstone(ThingWithComps gemstone) {
        if (!gemstone.HasComp<SprenContainer>()) return;
        insertedGemstone = gemstone;
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
        if (filterListInt.Count == 0 && Core.ThingDefOf.CutGem != null) {
            filterListInt.Add(Core.ThingDefOf.CutGem);
        }
        if (insertedGemstone != null) {
            CultivationSprenPatch.RegisterBuilding((FabrialBasicDiminisher)parent);
        }
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Deep.Look(ref insertedGemstone, "insertedGemstone");
        Scribe_Values.Look(ref powerOn, "PowerOn");
        Scribe_Values.Look(ref tempWhenTurnedOn, "TempWhenTurnedOn");
        Scribe_Collections.Look(ref filterListInt, "filterList", LookMode.Def);
    }

    public void CheckPower(bool flickeredOn) {
        InvestitureHolder? investiture = insertedGemstone?.TryGetComp<InvestitureHolder>();
        if (investiture != null) {
            bool power = investiture.currentInvestiture > 0 && flickeredOn;
            if (!powerOn && power) {
                tempWhenTurnedOn = parent.GetRoom().Temperature;
            }
            powerOn = power;
            return;
        }
        powerOn = false;
    }

    public void UsePower() {
        if (!powerOn) return;
        InvestitureHolder? investiture = insertedGemstone?.TryGetComp<InvestitureHolder>();
        if (investiture == null) return;

        investiture.drainRate = 1.0f;

        switch (insertedGemstone!.TryGetComp<SprenContainer>()?.CapturedSprenType) {
            case SprenType.Flamespren:
                DoFlameSprenPower();
                break;
            case SprenType.Rainspren:
                DoColdSprenPower();
                break;
            case SprenType.Fearspren:
                DoPainSprenPower();
                break;
            case SprenType.Lifespren:
                // Handled by patch
                break;
        }
    }

    private void DoFlameSprenPower() {
        if (!powerOn || parent.IsOutside()) return;
        InvestitureHolder? investiture = insertedGemstone?.TryGetComp<InvestitureHolder>();
        if (investiture == null) return;
        float gemstoneSize = investiture.maxInvestitureSelf * 3f;
        float currentTemp = parent.GetRoom().Temperature;
        if (currentTemp > tempWhenTurnedOn) {
            GenTemperature.PushHeat(parent.Position, parent.Map, 0f - gemstoneSize);
        }
    }

    private void DoColdSprenPower() {
        if (!powerOn || parent.IsOutside()) return;

        InvestitureHolder? investiture = insertedGemstone?.TryGetComp<InvestitureHolder>();
        if (investiture == null) return;
        float gemstoneSize = investiture.maxInvestitureSelf * 3f;
        float targetTemp = tempWhenTurnedOn;
        float currentTemp = parent.GetRoom().Temperature;
        if (currentTemp < targetTemp) {
            GenTemperature.PushHeat(parent.Position, parent.Map, gemstoneSize);
        }
    }

    private void DoPainSprenPower() {
        if (!powerOn) return;

        IntVec3 position = parent.Position;
        Verse.Map map = parent.Map;
        IEnumerable<IntVec3> cells = GenRadial.RadialCellsAround(position, 5f, true);
        foreach (IntVec3 cell in cells) {
            Pawn pawn = cell.GetFirstPawn(map);
            if (pawn != null &&
                pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Cosmere_Roshar_Painrial_Diminisher) == null &&
                pawn.Position.InHorDistOf(position, 5f)) {
                pawn.health.AddHediff(HediffDefOf.Cosmere_Roshar_Painrial_Diminisher);
            }
        }
    }

    public override string CompInspectStringExtra() {
        if (insertedGemstone == null) return "No gem in fabrial.";

        SprenContainer? sprenContainer = insertedGemstone.TryGetComp<SprenContainer>();
        InvestitureHolder? investiture = insertedGemstone.TryGetComp<InvestitureHolder>();

        return "Spren: " + (sprenContainer?.CapturedSprenType?.ToString() ?? "None") +
               "\nStormlight: " + (investiture?.currentInvestiture.ToString("F0") ?? "0") +
               "\ntime remaining: " + GetTimeRemaining();
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
        Verse.Thing? cutGemstone = GenClosest.ClosestThing_Global(
            selPawn.Position,
            selPawn.Map.listerThings.AllThings.Where(thing =>
                thing.HasComp<SprenContainer>() &&
                thing.TryGetComp<SprenContainer>().hasCapturedSpren &&
                thing.TryGetComp<InvestitureHolder>()?.currentInvestiture > 0 &&
                filterList.Contains(thing.def)
            ),
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
            action = () => { Find.WindowStack.Add(new SphereFilter<BasicFabrialDiminisher>(this)); },
        };
    }
}
