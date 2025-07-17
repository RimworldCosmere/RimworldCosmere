using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using System.Reflection;

namespace CosmereRoshar {
    public class CompSpherePouch : ThingComp {
        public List<Thing> storedSpheres = new List<Thing>(); //  List of sphere stacks inside the pouch
        private List<int> spheresToRemove = new List<int>();  //  spheres to remove

        public bool Empty => storedSpheres.Count == 0;


        public CompProperties_SpherePouch Props => (CompProperties_SpherePouch)props;
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Collections.Look(ref storedSpheres, "storedSpheres", LookMode.Deep);
        }

        //  Get total Stormlight from all stored spheres
        public float GetTotalStoredStormlight() {
            float total = 0;
            foreach (ThingWithComps sphere in storedSpheres) {
                CompStormlight comp = sphere.GetComp<CompStormlight>();
                if (comp != null) {
                    total += comp.Stormlight;
                }
            }
            return total;
        }

        public float GetTotalMaximumStormlight() {
            float total = 0;
            foreach (ThingWithComps sphere in storedSpheres) {
                CompStormlight comp = sphere.GetComp<CompStormlight>();
                if (comp != null) {
                    total += comp.MaxStormlightPerItem;
                }
            }
            return total;
        }

        public bool PouchContainsSpecificSphere(Thing sphere) {
            return storedSpheres.Contains(sphere);
        }

        public Thing GetSphereWithMostStormlight(bool addToRemoveList = false) {
            Thing sphere = null;
            int selectedIndex = 0;
            for (int i = 0; i < storedSpheres.Count; i++) {
                if (sphere == null || (storedSpheres[i].TryGetComp<CompStormlight>().Stormlight > sphere.TryGetComp<CompStormlight>().Stormlight)) {
                    sphere = storedSpheres[i];
                    selectedIndex = i;
                }
            }
            if (addToRemoveList && !spheresToRemove.Any(i => i == selectedIndex)) {
                spheresToRemove.Add(selectedIndex);
            }
            return sphere;
        }

        public void removeSpheresFromRemoveList() {
            spheresToRemove.Sort();
            spheresToRemove.Reverse();
            foreach (int i in spheresToRemove) {
                if (i < storedSpheres.Count) {
                    storedSpheres.RemoveAt(i);
                }
            }
            spheresToRemove.Clear();
        }

        public override string CompInspectStringExtra() {
            return "Stormlight: " + GetTotalStoredStormlight().ToString("F0") + " / " + GetTotalMaximumStormlight().ToString("F0");
        }

        public void InfuseStormlight(float amount) {
            foreach (ThingWithComps sphere in storedSpheres) {
                CompStormlight comp = sphere.GetComp<CompStormlight>();
                if (comp != null) {
                    comp.infuseStormlight(amount);
                }
            }
        }

        //  Absorb Stormlight from pouch
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

        public void RemoveSphereFromPouch(int index, Pawn pawn) {
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

        public bool RemoveSphereFromPouch(Thing sphere, Map map, IntVec3 dropPosition) {
            if (storedSpheres.Contains(sphere)) {
                storedSpheres.Remove(sphere);
                return GenPlace.TryPlaceThing(sphere, dropPosition, map, ThingPlaceMode.Near);
            }
            return false;
        }


        public bool AddSphereToPouch(ThingWithComps sphere) {
            if (sphere == null) {
                return false;
            }

            if (storedSpheres.Count >= Props.maxCapacity) { // Adjust max capacity as needed
                return false;
            }

            storedSpheres.Add(sphere);
            return true;
        }
        public static CompSpherePouch GetWornSpherePouch(Pawn pawn) {
            if (pawn.apparel == null) return null;

            foreach (Apparel apparel in pawn.apparel.WornApparel) {
                CompSpherePouch pouch = apparel.TryGetComp<CompSpherePouch>();
                if (pouch != null) {
                    return pouch; // Found a Sphere Pouch!
                }
            }
            return null; // Pawn is not wearing a Sphere Pouch
        }
    }
}
namespace CosmereRoshar {
    public class CompProperties_SpherePouch : CompProperties {
        public int maxCapacity;
        public List<ThingDef> allowedSpheres;

        public CompProperties_SpherePouch() {
            this.compClass = typeof(CompSpherePouch);
        }
    }
}

