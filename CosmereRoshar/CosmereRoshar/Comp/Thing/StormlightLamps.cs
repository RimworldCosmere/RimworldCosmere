using System;
using System.Collections.Generic;
using System.Linq;
using CosmereRoshar.Dialog;
using RimWorld;
using Verse;

namespace CosmereRoshar.Comp.Thing;

public class StormlightLampsProperties : CompProperties {
    public List<ThingDef>? allowedSpheres;
    public int? maxCapacity;

    public StormlightLampsProperties() {
        compClass = typeof(StormlightLamps);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef) {
        foreach (string configError in base.ConfigErrors(parentDef)) {
            yield return configError;
        }

        if (allowedSpheres.EnumerableNullOrEmpty()) {
            yield return "Allowed spheres must be set.";
        }

        if (maxCapacity is null or < 1) {
            yield return "Max capacity must be greater than 0.";
        }
    }
}

public class StormlightLamps : ThingComp, IFilterableComp {
    private bool deregisterAfterNewSphere = true;

    private bool initSphereAdded = false;

    private bool lightEnabled = true;
    public float mCurrentStormlight;

    private List<GemSize> sizeFilterListInt = [
        GemSize.Chip,
        GemSize.Mark,
        GemSize.Broam,
    ];

    public List<ThingWithComps> storedSpheres = []; // List of sphere stacks inside the pouch


    public new StormlightLampsProperties props => (StormlightLampsProperties)base.props;
    public CompGlower? glowerComp => parent.TryGetComp<CompGlower>();

    public float stormlightThresholdForRefuel { get; set; }

    public bool empty => storedSpheres.Count == 0;
    public List<ThingDef> filterList { get; private set; } = [];

    public List<ThingDef> allowedSpheres => props.allowedSpheres!;
    public List<GemSize> sizeFilterList => sizeFilterListInt;


    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);

        filterList = allowedSpheres.ToList();
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Collections.Look(ref storedSpheres, "storedSpheres", LookMode.Deep);
        Scribe_Collections.Look(ref sizeFilterListInt, "SizeFilterList", LookMode.Deep);
    }

    public bool IsFull() {
        if (empty) {
            return false;
        }

        ThingWithComps sphere = storedSpheres.ElementAt(0);
        if (!sphere.TryGetComp(out Stormlight stormlight)) return false;

        return storedSpheres.Count >= props.maxCapacity && stormlight.hasStormlight;
    }

    public int GetNumberOfSpheresToReplace() {
        //if (storedSpheres.Count() == 0)
        //    return 1;

        return 1; //make more advanced later
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra() {
        foreach (Gizmo gizmo in base.CompGetGizmosExtra()) {
            yield return gizmo;
        }

        yield return new Command_Action {
            defaultLabel = "Set Sphere Filters",
            defaultDesc = "Click to choose which spheres are allowed in this lamp.",
            //icon = ContentFinder<Texture2D>.Get("UI/Icons/SomeIcon"), 
            icon = TexCommand.DesirePower,
            action = () => { Find.WindowStack.Add(new SphereFilter<StormlightLamps>(this)); },
        };
    }


    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn) {
        foreach (ThingDef? element in filterList) {
            Log.Message($"{element.defName}");
        }

        foreach (GemSize element in sizeFilterListInt) {
            Log.Message($"{element.ToStringSafe()}");
        }

        Action? removeSphereAction = null;

        if (storedSpheres.Count > 0) {
            removeSphereAction = () => { RemoveSphereFromLamp(0); };
        }

        yield return new FloatMenuOption("Remove Sphere", removeSphereAction);


        Verse.Thing? sphere = null;
        SpherePouch spherePouch = SpherePouch.GetWornSpherePouch(selPawn);
        ThingDef matchingSphereDef = filterList.Find(def => selPawn.Map.listerThings.ThingsOfDef(def).Any());
        Verse.Thing? closestSphere = GenClosest.ClosestThing_Global(
            selPawn.Position,
            selPawn.Map.listerThings.AllThings
                .Where(thing =>
                    filterList.Contains(thing.def) &&
                    sizeFilterList.Contains(thing.TryGetComp<GemSphere>()?.GetGemSize() ?? GemSize.None)
                ),
            500f
        );
        if (!spherePouch.empty) {
            sphere = spherePouch.GetSphereWithMostStormlight(true);
        } else if (matchingSphereDef != null && closestSphere != null) {
            sphere = closestSphere;
        }

        Action? replaceSphereAction = null;
        string replaceSphereText = "No suitable sphere available";
        if (sphere != null) {
            Stormlight stormlight = sphere.TryGetComp<Stormlight>();

            if (stormlight.currentStormlight < stormlightThresholdForRefuel) {
                replaceSphereText = "Your available spheres is below the minimum stormlight threshold";
            } else if (filterList.Count == 0) {
                replaceSphereText = "No available spheres in list filter";
            } else {
                replaceSphereAction = () => {
                    Verse.AI.Job job = JobMaker.MakeJob(
                        CosmereRosharDefs.Cosmere_Roshar_RefuelSphereLamp,
                        parent,
                        sphere
                    );
                    if (job.TryMakePreToilReservations(selPawn, true)) {
                        selPawn.jobs.TryTakeOrderedJob(job);
                    }
                };
                replaceSphereText = $"Replace with {sphere.Label}";
            }
        }

        yield return new FloatMenuOption(replaceSphereText, replaceSphereAction);
    }


    public override void CompTick() {
        //if (storedSpheres.Count <= 0 && initSphereAdded == false) {
        //    Thing sphere = ThingMaker.MakeThing(ThingDef.Named("Sphere_Emerald"));
        //    ThingWithComps sphereComp = sphere as ThingWithComps;

        //    Stormlight comp = sphereComp.GetComp<Stormlight>();
        //    if (comp != null) {
        //        comp.Initialize(new StormlightProperties {
        //            maxStormlight = 1000f,
        //            drainRate = 0.01f
        //        });
        //    }

        //    AddSphereToLamp(sphereComp);
        //    initSphereAdded = true;
        //}
        CheckStormlightFuel();
    }

    public float DrawStormlight(float amount) {
        float absorbed = 0;
        foreach (ThingWithComps sphere in storedSpheres) {
            if (!sphere.TryGetComp(out Stormlight stormlight) || !stormlight.hasStormlight) continue;
            float drawn = stormlight.DrawStormlight(amount - absorbed);
            absorbed += drawn;
            if (absorbed >= amount) break; // Stop once fully absorbed
        }

        return absorbed;
    }

    public void CheckStormlightFuel() {
        float total = 0;
        foreach (ThingWithComps sphere in storedSpheres.Cast<ThingWithComps>()) {
            if (!sphere.TryGetComp(out Stormlight stormlight)) continue;
            stormlight.CompTick();
            total += stormlight.currentStormlight;
        }

        mCurrentStormlight = total;
        ToggleGlow(total > 0f);
    }

    public override string CompInspectStringExtra() {
        if (empty) return "No spheres in lamp.";

        ThingWithComps sphere = storedSpheres.First();
        return sphere.GetComp<GemSphere>().getFullLabel + "(" + mCurrentStormlight.ToString("F0") + ")";
    }

    public void InfuseStormlight(float amount) {
        foreach (ThingWithComps sphere in storedSpheres) {
            if (!sphere.TryGetComp(out Stormlight stormlight)) continue;
            stormlight.InfuseStormlight(amount);
        }
    }

    public bool AddSphereToLamp(ThingWithComps? sphere) {
        if (sphere == null) {
            return false;
        }

        if (storedSpheres.Count >= props.maxCapacity) {
            // Adjust max capacity as needed
            return false;
        }

        if (!sphere.TryGetComp(out Stormlight stormlight)) return false;
        if (!sphere.TryGetComp(out CompGlower glower)) return false;

        storedSpheres.Add(sphere);
        if (glowerComp != null) {
            glowerComp.GlowColor = glower.GlowColor;
            glowerComp.GlowRadius = stormlight.maximumGlowRadius;
        }

        deregisterAfterNewSphere = true;
        return true;
    }

    public void RemoveSphereFromLamp(int index, Pawn? pawn) {
        if (index < 0 || index >= storedSpheres.Count) {
            return;
        }

        ThingWithComps sphere = storedSpheres[index];
        storedSpheres.RemoveAt(index);

        if (sphere == null || pawn is not { Map: not null }) return;

        IntVec3 dropPosition = pawn.Position; // Drop at the pawn's current position
        GenPlace.TryPlaceThing(sphere, dropPosition, pawn.Map, ThingPlaceMode.Near);
    }

    public ThingWithComps? RemoveSphereFromLamp(int index, bool dropOnGround = true) {
        if (index < 0 || index >= storedSpheres.Count) {
            return null;
        }

        ThingWithComps sphere = storedSpheres[index];
        storedSpheres.RemoveAt(index);

        if (!dropOnGround || sphere == null || parent.Map == null) return sphere;

        IntVec3 dropPosition = parent.Position;
        GenPlace.TryPlaceThing(sphere, dropPosition, parent.Map, ThingPlaceMode.Near);

        return sphere;
    }

    private void ToggleGlow(bool toggleOn) {
        if (parent.Map != null && lightEnabled != toggleOn) {
            lightEnabled = toggleOn;
            if (toggleOn) {
                parent.Map.glowGrid.RegisterGlower(glowerComp);
            } else {
                parent.Map.glowGrid.DeRegisterGlower(glowerComp);
            }
        }

        if (lightEnabled || !deregisterAfterNewSphere) return;
        deregisterAfterNewSphere = false;
        parent.Map?.glowGrid.DeRegisterGlower(glowerComp);
    }
}