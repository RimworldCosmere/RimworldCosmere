using System;
using System.Collections.Generic;
using System.Linq;
using CosmereRoshar.Comp;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Comps.Apparel;
using CosmereRoshar.Comps.Gems;
using CosmereRoshar.ITabs;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Comps.Furniture;

public class StormlightLamps : ThingComp, IFilterableComp {
    private bool deregisterAfterNewSphere = true;

    private bool initSphereAdded = false;


    private bool lightEnabled = true;
    public float mCurrentStormlight;

    private List<GemSize> sizeFilterListInt = new List<GemSize> {
        GemSize.Chip,
        GemSize.Mark,
        GemSize.Broam,
    };

    public List<Thing> storedSpheres = new List<Thing>(); // List of sphere stacks inside the pouch


    public CompPropertiesSphereLamp props => (CompPropertiesSphereLamp)base.props;
    public CompGlower glowerComp => parent.GetComp<CompGlower>();

    public float stormlightThresholdForRefuel { get; set; }

    public bool empty => storedSpheres.Count == 0;
    public List<ThingDef> filterList { get; private set; } = new List<ThingDef>();

    public List<ThingDef> allowedSpheres => props.allowedSpheres;
    public List<GemSize> sizeFilterList => sizeFilterListInt;


    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);
        if (filterList == null && props.allowedSpheres != null) {
            filterList = props.allowedSpheres.ToList();
        }
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

        ThingWithComps sphere = storedSpheres.ElementAt(0) as ThingWithComps;
        Stormlight comp = sphere.GetComp<Stormlight>();
        return storedSpheres.Count() >= props.maxCapacity && comp.hasStormlight;
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
            action = () => { Find.WindowStack.Add(new DialogSphereFilter<StormlightLamps>(this)); },
        };
    }


    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn) {
        foreach (ThingDef? element in filterList) {
            Log.Message($"{element.defName}");
        }

        foreach (GemSize element in sizeFilterListInt) {
            Log.Message($"{element.ToStringSafe()}");
        }

        Action removeSphereAction = null;

        if (storedSpheres.Count > 0) {
            removeSphereAction = () => { RemoveSphereFromLamp(0); };
        }

        yield return new FloatMenuOption("Remove Sphere", removeSphereAction);


        Thing? sphere = null;
        CompSpherePouch spherePouch = CompSpherePouch.GetWornSpherePouch(selPawn);
        ThingDef matchingSphereDef = filterList.Find(def => selPawn.Map.listerThings.ThingsOfDef(def).Any());
        Thing? closestSphere = GenClosest.ClosestThing_Global(
            selPawn.Position,
            selPawn.Map.listerThings.AllThings
                .Where(thing =>
                    filterList.Contains(thing.def) &&
                    sizeFilterList.Contains(thing.TryGetComp<CompGemSphere>()?.GetGemSize() ?? GemSize.None)
                ),
            500f
        );
        if (!spherePouch.empty) {
            sphere = spherePouch.GetSphereWithMostStormlight(true);
        } else if (matchingSphereDef != null && closestSphere != null) {
            sphere = closestSphere;
        }

        Action replaceSphereAction = null;
        string replaceSphereText = "No suitable sphere available";
        if (sphere != null) {
            Stormlight stormlight = sphere.TryGetComp<Stormlight>();

            if (stormlight.currentStormlight < stormlightThresholdForRefuel) {
                replaceSphereText = "Your available spheres is below the minimum stormlight threshold";
            } else if (filterList.Count == 0) {
                replaceSphereText = "No available spheres in list filter";
            } else {
                replaceSphereAction = () => {
                    Job job = JobMaker.MakeJob(CosmereRosharDefs.WhtwlRefuelSphereLamp, parent, sphere);
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
            Stormlight comp = sphere.GetComp<Stormlight>();
            if (comp != null && comp.hasStormlight) {
                float drawn = comp.DrawStormlight(amount - absorbed);
                absorbed += drawn;
                if (absorbed >= amount) break; // Stop once fully absorbed
            }
        }

        return absorbed;
    }

    public void CheckStormlightFuel() {
        float total = 0;
        foreach (ThingWithComps sphere in storedSpheres) {
            Stormlight comp = sphere.GetComp<Stormlight>();
            comp.CompTick();
            if (comp != null) {
                total += comp.currentStormlight;
            }
        }

        mCurrentStormlight = total;
        ToggleGlow(total > 0f);
    }

    public override string CompInspectStringExtra() {
        string sphereName = "No sphere in lamp.";
        if (storedSpheres.Count > 0) {
            ThingWithComps sphere = storedSpheres.First() as ThingWithComps;
            sphereName = sphere.GetComp<CompGemSphere>().getFullLabel + "(" + mCurrentStormlight.ToString("F0") + ")";
        }

        return sphereName;
    }

    public void InfuseStormlight(float amount) {
        foreach (ThingWithComps sphere in storedSpheres) {
            Stormlight comp = sphere.GetComp<Stormlight>();
            if (comp != null) {
                comp.InfuseStormlight(amount);
            }
        }
    }

    public bool AddSphereToLamp(ThingWithComps sphere) {
        if (sphere == null) {
            return false;
        }

        if (storedSpheres.Count >= props.maxCapacity) {
            // Adjust max capacity as needed
            return false;
        }

        Stormlight stormlight = sphere.GetComp<Stormlight>();
        if (stormlight == null) return false;

        storedSpheres.Add(sphere);
        glowerComp.GlowColor = stormlight.glowerComp.GlowColor;
        glowerComp.GlowRadius = stormlight.maximumGlowRadius;

        deregisterAfterNewSphere = true;
        return true;
    }

    public void RemoveSphereFromLamp(int index, Pawn pawn) {
        if (index < 0 || index >= storedSpheres.Count) {
            return;
        }

        ThingWithComps sphere = storedSpheres[index] as ThingWithComps;

        storedSpheres.RemoveAt(index);

        if (sphere != null && pawn != null && pawn.Map != null) {
            IntVec3 dropPosition = pawn.Position; // Drop at the pawn's current position
            GenPlace.TryPlaceThing(sphere, dropPosition, pawn.Map, ThingPlaceMode.Near);
        }
    }

    public ThingWithComps RemoveSphereFromLamp(int index, bool dropOnGround = true) {
        if (index < 0 || index >= storedSpheres.Count) {
            return null;
        }

        ThingWithComps sphere = storedSpheres[index] as ThingWithComps;

        storedSpheres.RemoveAt(index);

        if (dropOnGround && sphere != null && parent.Map != null) {
            IntVec3 dropPosition = parent.Position;
            GenPlace.TryPlaceThing(sphere, dropPosition, parent.Map, ThingPlaceMode.Near);
        }

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

        if (!lightEnabled && deregisterAfterNewSphere) {
            deregisterAfterNewSphere = false;
            parent.Map.glowGrid.DeRegisterGlower(glowerComp);
        }
    }
}

public class CompPropertiesSphereLamp : CompProperties {
    public List<ThingDef> allowedSpheres;
    public int maxCapacity;

    public CompPropertiesSphereLamp() {
        compClass = typeof(StormlightLamps);
    }
}