using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using Verse.AI;
using Unity.Jobs;
using UnityEngine;

namespace CosmereRoshar {
    public class StormlightLamps : ThingComp, IFilterableComp {
     
        public CompProperties_SphereLamp Props => (CompProperties_SphereLamp)props;
        public CompGlower GlowerComp => parent.GetComp<CompGlower>();
        public List<Thing> storedSpheres = new List<Thing>(); // List of sphere stacks inside the pouch

        private List<ThingDef> filterList = new List<ThingDef>();
        public List<ThingDef> FilterList => filterList; 
        public List<ThingDef> AllowedSpheres => Props.allowedSpheres;

        private List<GemSize> sizeFilterList = new List<GemSize>()
        {
            GemSize.Chip,
            GemSize.Mark,
            GemSize.Broam
        };
        public List<GemSize> SizeFilterList => sizeFilterList;

      
        private float stormlightThresholdForRefuel = 0f; 
        public float StormlightThresholdForRefuel {
            get { return stormlightThresholdForRefuel; }
            set { stormlightThresholdForRefuel = value; }
        }


        bool lightEnabled = true;
        bool initSphereAdded = false;
        public bool Empty => storedSpheres.Count == 0;
        public float m_CurrentStormlight = 0f;
        private bool deregisterAfterNewSphere = true;


        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
            if (filterList == null && Props.allowedSpheres != null) {
                filterList = Props.allowedSpheres.ToList();
            }
        }
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Collections.Look(ref storedSpheres, "storedSpheres", LookMode.Deep);
            Scribe_Collections.Look(ref sizeFilterList, "SizeFilterList", LookMode.Deep);
        }

        public bool IsFull() {
            if (Empty)
                return false;
            ThingWithComps sphere = storedSpheres.ElementAt(0) as ThingWithComps;
            CompStormlight comp = sphere.GetComp<CompStormlight>();
            return storedSpheres.Count() >= Props.maxCapacity && comp.HasStormlight;
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
                action = () => {
                    Find.WindowStack.Add(new Dialog_SphereFilter<StormlightLamps>(this));
                }
            };
        }


        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn) {
            foreach(var element in FilterList) { Log.Message($"{element.defName}"); }
            foreach(var element in sizeFilterList) { Log.Message($"{element.ToStringSafe()}"); }
            
            Action removeSphereAction = null;

            if (storedSpheres.Count() > 0) {
                removeSphereAction = () => {
                    RemoveSphereFromLamp(0);
                };
            }
            yield return new FloatMenuOption("Remove Sphere", removeSphereAction);


            Thing sphere = null;
            CompSpherePouch spherePouch = CompSpherePouch.GetWornSpherePouch(selPawn);
            ThingDef matchingSphereDef = filterList.Find(def => selPawn.Map.listerThings.ThingsOfDef(def).Any());
            var closestSphere = GenClosest.ClosestThing_Global(
                   selPawn.Position,
                   selPawn.Map.listerThings.AllThings
                       .Where(thing =>
                           FilterList.Contains(thing.def) &&  
                           SizeFilterList.Contains(thing.TryGetComp<CompGemSphere>()?.GetGemSize() ?? GemSize.None) 
                       ),
                   500f);
            if (spherePouch != null && !spherePouch.Empty) {
                sphere = spherePouch.GetSphereWithMostStormlight(true);
            }
            else if (matchingSphereDef != null && closestSphere != null) {
                sphere = closestSphere;
            }

            Action replaceSphereAction = null;
            string replaceSphereText = "No suitable sphere available";
            if (sphere != null) {
                CompStormlight sphereComp = sphere.TryGetComp<CompStormlight>();

                if (sphereComp.Stormlight < stormlightThresholdForRefuel) {
                    replaceSphereText = $"Your available spheres is below the minimum stormlight threshold";
                }
                else if (FilterList.Count == 0) {
                    replaceSphereText = $"No available spheres in list filter";
                }
                else {
                    replaceSphereAction = () => {
                        Job job = JobMaker.MakeJob(CosmereRosharDefs.whtwl_RefuelSphereLamp, parent, sphere);
                        if (job.TryMakePreToilReservations(selPawn, errorOnFailed: true)) {
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

            //    CompStormlight comp = sphereComp.GetComp<CompStormlight>();
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
                CompStormlight comp = sphere.GetComp<CompStormlight>();
                if (comp != null && comp.HasStormlight) {
                    float drawn = comp.drawStormlight(amount - absorbed);
                    absorbed += drawn;
                    if (absorbed >= amount) break; // Stop once fully absorbed
                }
            }
            return absorbed;
        }

        public void CheckStormlightFuel() {
            float total = 0;
            foreach (ThingWithComps sphere in storedSpheres) {
                CompStormlight comp = sphere.GetComp<CompStormlight>();
                comp.CompTick();
                if (comp != null) {
                    total += comp.Stormlight;
                }
            }
            m_CurrentStormlight = total;
            toggleGlow(total > 0f);
        }

        public override string CompInspectStringExtra() {
            string sphereName = "No sphere in lamp.";
            if (storedSpheres.Count > 0) {
                ThingWithComps sphere = storedSpheres.First() as ThingWithComps;
                sphereName = sphere.GetComp<CompGemSphere>().GetFullLabel + "(" + m_CurrentStormlight.ToString("F0") + ")";
            }
            return sphereName;
        }

        public void InfuseStormlight(float amount) {
            foreach (ThingWithComps sphere in storedSpheres) {
                CompStormlight comp = sphere.GetComp<CompStormlight>();
                if (comp != null) {
                    comp.infuseStormlight(amount);
                }
            }
        }

        public bool AddSphereToLamp(ThingWithComps sphere) {
            if (sphere == null) {
                return false;
            }

            if (storedSpheres.Count >= Props.maxCapacity) { // Adjust max capacity as needed
                return false;
            }
            CompStormlight stormlightComp = sphere.GetComp<CompStormlight>();
            if (stormlightComp == null) return false;

            storedSpheres.Add(sphere);
            GlowerComp.GlowColor = stormlightComp.GlowerComp.GlowColor;
            GlowerComp.GlowRadius = stormlightComp.MaximumGlowRadius;

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
        private void toggleGlow(bool toggleOn) {
            if (parent.Map != null && lightEnabled != toggleOn) {
                lightEnabled = toggleOn;
                if (toggleOn) {
                    parent.Map.glowGrid.RegisterGlower(GlowerComp);
                }
                else {
                    parent.Map.glowGrid.DeRegisterGlower(GlowerComp);
                }
            }
            if (!lightEnabled && deregisterAfterNewSphere) {
                deregisterAfterNewSphere = false;
                parent.Map.glowGrid.DeRegisterGlower(GlowerComp);

            }
        }
    }

    public class CompProperties_SphereLamp : CompProperties {
        public int maxCapacity;
        public List<ThingDef> allowedSpheres;

        public CompProperties_SphereLamp() {
            this.compClass = typeof(StormlightLamps);
        }
    }
}

