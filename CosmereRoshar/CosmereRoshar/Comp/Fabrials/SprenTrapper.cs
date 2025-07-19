using System;
using System.Collections.Generic;
using System.Linq;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Dialog;
using CosmereRoshar.Thing;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereRoshar.Comp.Fabrials;

public class SprenTrapper : ThingComp, IGemstoneHandler, IFilterableComp {
    private List<ThingDef> filterListInt = [];
    public ThingWithComps? insertedGemstone;

    private List<GemSize> sizeFilterListInt = [
        GemSize.Chip,
        GemSize.Mark,
        GemSize.Broam,
    ];

    public bool sprenCaptured;


    public bool hasGemstone => insertedGemstone != null;

    public float stormlightThresholdForRefuel { get; set; }

    public List<GemSize> sizeFilterList => sizeFilterListInt;

    public List<ThingDef> filterList => filterListInt;

    public List<ThingDef> allowedSpheres { get; } = [
        CosmereResources.ThingDefOf.CutDiamond,
        CosmereResources.ThingDefOf.CutGarnet,
        CosmereResources.ThingDefOf.CutRuby,
        CosmereResources.ThingDefOf.CutSapphire,
        CosmereResources.ThingDefOf.CutEmerald,
    ];

    public void AddGemstone(ThingWithComps gemstone) {
        CompCutGemstone? gemstoneComp = gemstone.GetComp<CompCutGemstone>();
        if (gemstoneComp != null) {
            insertedGemstone = gemstoneComp.parent;
        }
    }

    public void RemoveGemstone() {
        if (insertedGemstone != null) {
            Verse.Thing gemstoneToDrop = insertedGemstone;
            insertedGemstone = null;
            IntVec3 dropPosition = parent.Position;
            dropPosition.z -= 1;
            Map? map = parent.Map;

            if (map == null) {
                map = Find.CurrentMap;
            }

            if (map != null) {
                GenPlace.TryPlaceThing(gemstoneToDrop, dropPosition, map, ThingPlaceMode.Near);
            }
        }
    }


    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);
    }


    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Deep.Look(ref insertedGemstone, "insertedGemstone");
        Scribe_Values.Look(ref sprenCaptured, "sprenCaptured");
        Scribe_Collections.Look(ref sizeFilterListInt, "sizeFilterList", LookMode.Value);
        Scribe_Collections.Look(ref filterListInt, "filterList", LookMode.Def);
    }

    public void TryCaptureSpren(Spren targetSpren) {
        if (insertedGemstone == null || targetSpren == Spren.None || sprenCaptured) return;
        switch (targetSpren) {
            case Spren.Flame:
                TryCaptureFlameSpren();
                break;
            case Spren.Cold:
                TryCaptureColdSpren();
                break;
            case Spren.Pain:
                TryCapturePainSpren();
                break;
            case Spren.Life:
                TryCaptureLifeSpren();
                break;
            case Spren.Logic:
                TryCaptureLogicSpren();
                break;
        }
    }

    private void DisplayCaptureMessage(Spren spren) {
        (parent as BuildingSprenTrapper).TriggerPrint();
        Messages.Message(
            $"One of your traps captured a {spren.ToStringSafe()}spren!",
            parent,
            MessageTypeDefOf.PositiveEvent
        );
    }

    private float GetProbability(Stormlight stormlight, float probabilityFactor, float baseMaxNormalizedValue) {
        float baseProbability = StormlightUtilities.SprenBaseCaptureProbability(
            stormlight.currentStormlight,
            0f,
            stormlight.currentMaxStormlight
        );
        float probability = StormlightUtilities.Normalize(baseProbability, 0f, 100f, 0f, baseMaxNormalizedValue);
        return probability * probabilityFactor;
    }

    private void TryCaptureFlameSpren() {
        if (StormlightUtilities.IsAnyFireNearby(parent as Building)) {
            Stormlight? stormlightcomp = insertedGemstone.TryGetComp<Stormlight>();
            if (stormlightcomp != null) {
                float probabilityFactor = StormlightUtilities.GetNumberOfFiresNearby(parent as Building, 3f);
                float probability = GetProbability(stormlightcomp, probabilityFactor, 0.1f);

                if (Rand.Chance(probability)) {
                    insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren = Spren.Flame;
                    stormlightcomp.RemoveAllStormlight();
                    DisplayCaptureMessage(Spren.Flame);
                }
            }
        }
    }

    private void TryCaptureColdSpren() {
        float averageSuroundingTemperature = StormlightUtilities.GetAverageSuroundingTemperature(parent as Building);
        if (averageSuroundingTemperature < 0f) {
            Stormlight? stormlightcomp = insertedGemstone.TryGetComp<Stormlight>();
            if (stormlightcomp != null) {
                float probabilityFactor = Mathf.Abs(averageSuroundingTemperature);
                float probability = GetProbability(stormlightcomp, probabilityFactor, 0.01f);

                if (Rand.Chance(probability)) {
                    insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren = Spren.Cold;
                    DisplayCaptureMessage(Spren.Cold);
                    stormlightcomp.RemoveAllStormlight();
                }
            }
        }
    }


    private void TryCapturePainSpren() {
        float sumOfPain = StormlightUtilities.GetSuroundingPain(parent as Building);
        if (sumOfPain > 0f) {
            Stormlight? stormlightcomp = insertedGemstone.TryGetComp<Stormlight>();
            if (stormlightcomp != null) {
                float probabilityFactor = sumOfPain * 5;
                float probability = GetProbability(stormlightcomp, probabilityFactor, 0.01f);

                if (Rand.Chance(probability)) {
                    insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren = Spren.Pain;
                    DisplayCaptureMessage(Spren.Pain);
                    stormlightcomp.RemoveAllStormlight();
                }
            }
        }
    }

    private void TryCaptureLifeSpren() {
        float sumOfPlants = StormlightUtilities.GetSuroundingPlants(parent as Building);
        if (sumOfPlants > 0) {
            Stormlight? stormlightcomp = insertedGemstone.TryGetComp<Stormlight>();
            if (stormlightcomp != null) {
                float probabilityFactor = sumOfPlants;
                float probability = GetProbability(stormlightcomp, probabilityFactor, 0.01f);

                if (Rand.Chance(probability)) {
                    insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren = Spren.Life;
                    DisplayCaptureMessage(Spren.Life);
                    stormlightcomp.RemoveAllStormlight();
                }
            }
        }
    }

    private void TryCaptureLogicSpren() {
        bool researchDoneNearby = StormlightUtilities.ResearchBeingDoneNearby(parent as Building);
        if (researchDoneNearby) {
            Stormlight? stormlightcomp = insertedGemstone.TryGetComp<Stormlight>();
            if (stormlightcomp != null) {
                float probabilityFactor = 2f;
                float probability = GetProbability(stormlightcomp, probabilityFactor, 0.0005f);
                if (Rand.Chance(probability)) {
                    insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren = Spren.Logic;
                    DisplayCaptureMessage(Spren.Logic);
                    stormlightcomp.RemoveAllStormlight();
                }
            }
        }
    }


    public void CheckTrapperState() {
        if (insertedGemstone?.TryGetComp<CompCutGemstone>() is { } compCutGemstone) {
            if (compCutGemstone.capturedSpren != Spren.None) {
                sprenCaptured = true;
                return;
            }
        }

        sprenCaptured = false;
    }

    public void DrainStormlight() {
        if (!(insertedGemstone?.TryGetComp(out Stormlight stormlight) ?? false)) return;
        stormlight.DrainStormLight();
        stormlight.DrainStormLight();
        stormlight.DrainStormLight();
        stormlight.DrainStormLight();
        stormlight.DrainStormLight();
    }

    public void InstallCage(ThingWithComps cage) { }


    public override string CompInspectStringExtra() {
        string gemName = "No gem in fabrial.";

        if (insertedGemstone == null) return gemName;
        ThingWithComps gemstone = insertedGemstone;
        gemName = gemstone.GetComp<CompCutGemstone>().getFullLabel +
                  "(" +
                  gemstone.GetComp<Stormlight>().currentStormlight.ToString("F0") +
                  ")";

        return gemName;
    }

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn) {
        Verse.Thing? cutGemstone = GenClosest.ClosestThing_Global(
            selPawn.Position,
            selPawn.Map.listerThings.AllThings.Where(thing =>
                StormlightUtilities.IsThingCutGemstone(thing) &&
                !thing.TryGetComp<CompCutGemstone>().hasSprenInside &&
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
                Verse.AI.Job job = JobMaker.MakeJob(CosmereRosharDefs.WhtwlRefuelFabrial, parent, cutGemstone);
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
                Verse.AI.Job job = JobMaker.MakeJob(CosmereRosharDefs.WhtwlRemoveFromFabrial, parent);
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
            defaultDesc = "Click to choose which gems are allowed in this trap.",
            //icon = ContentFinder<Texture2D>.Get("UI/Icons/SomeIcon"), 
            icon = TexCommand.SelectShelf,
            action = () => { Find.WindowStack.Add(new SphereFilter<SprenTrapper>(this)); },
        };
    }
}