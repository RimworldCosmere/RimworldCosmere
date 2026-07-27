using System;
using Cosmere.Core.Comp.Thing;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using Cosmere.System.Roshar.Patch.Fabrials;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Comp.Fabrials;

public abstract class BasicFabrial : ThingComp, IGemstoneHandler, IFilterableComp {
    private List<ThingDef> filterListInt = [];
    public ThingWithComps? insertedGemstone;
    public bool powerOn;

    public bool hasGemstone => insertedGemstone != null;

    private SprenContainer? Spren => insertedGemstone?.TryGetComp<SprenContainer>();

    private InvestitureHolder? Holder => insertedGemstone?.TryGetComp<InvestitureHolder>();

    public SprenType? currentSpren => Spren?.CapturedSprenType;

    protected abstract HediffDef PainHediffDef { get; }

    public List<ThingDef> FilterList => filterListInt;

    public List<ThingDef> AllowedSpheres { get; } = [
        Core.ThingDefOf.CutGem,
    ];

    public abstract void AddGemstone(ThingWithComps gemstone);

    public abstract void RemoveGemstone();

    protected abstract void ApplyFlameSprenHeat();

    protected abstract void ApplyColdSprenCooling();

    protected void RegisterBuilding() {
        CultivationSprenPatch.RegisterBuilding((Building)parent);
    }

    protected void UnregisterBuilding() {
        CultivationSprenPatch.UnregisterBuilding((Building)parent);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);
        if (filterListInt.Count == 0 && Core.ThingDefOf.CutGem != null) {
            filterListInt.Add(Core.ThingDefOf.CutGem);
        }

        if (insertedGemstone != null) {
            RegisterBuilding();
        }
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Deep.Look(ref insertedGemstone, "insertedGemstone");
        Scribe_Values.Look(ref powerOn, "PowerOn");
        Scribe_Collections.Look(ref filterListInt, "filterList", LookMode.Def);
        SaveExtraData();
    }

    protected virtual void SaveExtraData() { }

    public virtual void UpdatePowerState(bool flickeredOn) {
        InvestitureHolder? investiture = Holder;
        if (investiture != null) {
            powerOn = investiture.currentInvestiture > 0 && flickeredOn;
            return;
        }

        powerOn = false;
    }

    public void UsePower() {
        if (!powerOn) return;
        InvestitureHolder? investiture = Holder;
        if (investiture == null) return;

        investiture.drainRate = 1.0f;

        switch (Spren?.CapturedSprenType) {
            case SprenType.Flamespren:
                ApplyFlameSprenHeat();
                break;
            case SprenType.Rainspren:
                ApplyColdSprenCooling();
                break;
            case SprenType.Fearspren:
                DoPainSprenPower();
                break;
            case SprenType.Lifespren:
                break;
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
                pawn.health.hediffSet.GetFirstHediffOfDef(PainHediffDef) == null &&
                pawn.Position.InHorDistOf(position, 5f)) {
                pawn.health.AddHediff(PainHediffDef);
            }
        }
    }

    public override string CompInspectStringExtra() {
        if (insertedGemstone == null) return "No gem in fabrial.";

        SprenContainer? sprenContainer = Spren;
        InvestitureHolder? investiture = Holder;

        return "Spren: " +
               (sprenContainer?.CapturedSprenType?.ToString() ?? "None") +
               "\nStormlight: " +
               (investiture?.currentInvestiture.ToString("F0") ?? "0") +
               "\ntime remaining: " +
               GetTimeRemaining();
    }

    private string GetTimeRemaining() {
        InvestitureHolder? investiture = Holder;
        if (investiture == null) return "\u221e";

        float tickRaresPerHour = (float)GenDate.TicksPerHour / GenTicks.TickRareInterval;
        float investiturePerHour = investiture.drainRate * tickRaresPerHour;
        if (Mathf.Approximately(investiturePerHour, 0f)) return "\u221e";

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
                FilterList.Contains(thing.def)
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
            action = OpenFilterDialog,
        };
    }

    protected abstract void OpenFilterDialog();
}
