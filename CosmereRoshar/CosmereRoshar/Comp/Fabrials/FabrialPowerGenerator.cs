using System;
using System.Collections.Generic;
using System.Linq;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.ITabs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Comp.Fabrials;

public class BuildingFabrialPowerGenerator : Building {
    public CompFabrialPowerGenerator compFabrialPowerGenerator;
    public CompFlickable compFlickerable;
    public CompGlower compGlower;
    public CompPowerPlant compPowerPlant;


    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        compFabrialPowerGenerator = GetComp<CompFabrialPowerGenerator>();
        compFlickerable = GetComp<CompFlickable>();
        compGlower = GetComp<CompGlower>();
        compPowerPlant = GetComp<CompPowerPlant>();
    }

    protected override void Tick() {
        compFabrialPowerGenerator.CheckPower(compFlickerable.SwitchIsOn);
        if (!compFabrialPowerGenerator.powerOn) {
            compPowerPlant.PowerOutput = 0f;
        } else {
            compPowerPlant.PowerOutput = 1000f;
        } //maybe add quality of stone affects power output later

        ToggleGlow(compFabrialPowerGenerator.powerOn);
        compFabrialPowerGenerator.UsePower();
    }

    private void ToggleGlow(bool on) {
        if (Map != null) {
            if (on) {
                Stormlight? stormlightComp = compFabrialPowerGenerator.insertedGemstone
                    .GetComp<Stormlight>();
                compGlower.GlowRadius = stormlightComp.maximumGlowRadius;
                compGlower.GlowColor = stormlightComp.glowerComp.GlowColor;
                Map.glowGrid.RegisterGlower(compGlower);
            } else {
                Map.glowGrid.DeRegisterGlower(compGlower);
            }
        }
    }
}

public class CompPropertiesFabrialPowerGenerator : CompProperties {
    public CompPropertiesFabrialPowerGenerator() {
        compClass = typeof(CompFabrialPowerGenerator);
    }
}

public class CompFabrialPowerGenerator : ThingComp, IGemstoneHandler, IFilterableComp {
    private List<ThingDef> filterListInt = [];
    public ThingWithComps? insertedGemstone;
    public bool powerOn;

    private List<GemSize> sizeFilterListInt = [
        GemSize.Chip,
        GemSize.Mark,
        GemSize.Broam,
    ];

    public new CompPropertiesFabrialPowerGenerator props => (CompPropertiesFabrialPowerGenerator)base.props;
    public CompGlower? glowerComp => parent.TryGetComp<CompGlower>();
    public bool hasGemstone => insertedGemstone != null;
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
        if (gemstoneComp != null) {
            insertedGemstone = gemstoneComp.parent;
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
        if (insertedGemstone != null) { }
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Deep.Look(ref insertedGemstone, "insertedGemstone");
        Scribe_Values.Look(ref powerOn, "PowerOn");
        Scribe_Collections.Look(ref sizeFilterListInt, "sizeFilterList", LookMode.Value);
        Scribe_Collections.Look(ref filterListInt, "filterList", LookMode.Def);
    }

    public void CheckPower(bool flickeredOn) {
        if (insertedGemstone?.TryGetComp(out Stormlight stormlight) ?? false) {
            powerOn = stormlight.hasStormlight && flickeredOn;
            return;
        }

        powerOn = false;
    }


    public void UsePower() {
        if (!(insertedGemstone?.TryGetComp(out Stormlight stormlight) ?? false)) return;

        stormlight.drainFactor = powerOn ? 4f : 1f;
        stormlight.CompTick();
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
        float stormlightPerHour = 50.0f * stormlightComp.GetDrainRate(stormlightComp.drainFactor);
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
                thing.TryGetComp<Stormlight>().hasStormlight &&
                filterList.Contains(thing.def) &&
                sizeFilterList.Contains(thing.TryGetComp<CompCutGemstone>()?.GetGemSize() ?? GemSize.None)
            ),
            500f
        );


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
            icon = TexCommand.SelectShelf,
            action = () => { Find.WindowStack.Add(new DialogSphereFilter<CompFabrialPowerGenerator>(this)); },
        };
    }
}