using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CosmereRoshar.Comp.Thing;

public class SpherePouchProperties : CompProperties {
    public List<ThingDef>? allowedSpheres;
    public int? maxCapacity;

    public SpherePouchProperties() {
        compClass = typeof(SpherePouch);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef) {
        foreach (string error in base.ConfigErrors(parentDef)) {
            yield return error;
        }

        if (allowedSpheres.NullOrEmpty()) yield return "allowedSpheres is null or empty";
        if (maxCapacity is < 1) yield return "maxCapacity is less than 1";
    }
}

public class SpherePouch : ThingComp {
    private readonly List<int> spheresToRemove = []; //  spheres to remove
    public List<ThingWithComps> storedSpheres = []; //  List of sphere stacks inside the pouch

    public bool empty => storedSpheres.Count == 0;


    public new SpherePouchProperties props => (SpherePouchProperties)base.props;

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Collections.Look(ref storedSpheres, "storedSpheres", LookMode.Deep);
    }

    //  Get total Stormlight from all stored spheres
    public float GetTotalStoredStormlight() {
        float total = 0;
        foreach (ThingWithComps sphere in storedSpheres.Cast<ThingWithComps>()) {
            if (!sphere.TryGetComp(out Stormlight stormlight)) continue;
            total += stormlight.currentStormlight;
        }

        return total;
    }

    public float GetTotalMaximumStormlight() {
        float total = 0;
        foreach (ThingWithComps sphere in storedSpheres.Cast<ThingWithComps>()) {
            if (!sphere.TryGetComp(out Stormlight stormlight)) continue;
            total += stormlight.maxStormlightPerItem;
        }

        return total;
    }

    public bool PouchContainsSpecificSphere(Verse.Thing sphere) {
        return storedSpheres.Contains(sphere);
    }

    public Verse.Thing GetSphereWithMostStormlight(bool addToRemoveList = false) {
        Verse.Thing sphere = null!;
        int selectedIndex = 0;
        for (int i = 0; i < storedSpheres.Count; i++) {
            if (sphere != null &&
                !(storedSpheres[i].TryGetComp<Stormlight>().currentStormlight >
                  sphere.TryGetComp<Stormlight>().currentStormlight)) {
                continue;
            }

            sphere = storedSpheres[i];
            selectedIndex = i;
        }

        if (addToRemoveList && !spheresToRemove.Any(i => i == selectedIndex)) {
            spheresToRemove.Add(selectedIndex);
        }

        return sphere!;
    }

    public void RemoveSpheresFromRemoveList() {
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
        return "Stormlight: " +
               GetTotalStoredStormlight().ToString("F0") +
               " / " +
               GetTotalMaximumStormlight().ToString("F0");
    }

    public void InfuseStormlight(float amount) {
        foreach (ThingWithComps sphere in storedSpheres.Cast<ThingWithComps>()) {
            sphere.TryGetComp<Stormlight>().InfuseStormlight(amount);
        }
    }

    //  Absorb Stormlight from pouch
    public float DrawStormlight(float amount) {
        float absorbed = 0;

        foreach (ThingWithComps sphere in storedSpheres.Cast<ThingWithComps>()) {
            if (!sphere.TryGetComp(out Stormlight stormlight) || !stormlight.hasStormlight) continue;

            float drawn = stormlight.DrawStormlight(amount - absorbed);
            absorbed += drawn;
            if (absorbed >= amount) break; // Stop once fully absorbed
        }

        return absorbed;
    }

    public void RemoveSphereFromPouch(int index, Pawn? pawn) {
        if (index < 0 || index >= storedSpheres.Count) {
            return;
        }

        ThingWithComps sphere = storedSpheres[index];

        storedSpheres.RemoveAt(index);

        if (sphere == null || pawn?.Map == null) return;
        IntVec3 dropPosition = pawn.Position; // Drop at the pawn's current position
        GenPlace.TryPlaceThing(sphere, dropPosition, pawn.Map, ThingPlaceMode.Near);
    }

    public bool RemoveSphereFromPouch(ThingWithComps sphere, Map map, IntVec3 dropPosition) {
        if (!storedSpheres.Contains(sphere)) return false;

        storedSpheres.Remove(sphere);
        return GenPlace.TryPlaceThing(sphere, dropPosition, map, ThingPlaceMode.Near);
    }


    public bool AddSphereToPouch(ThingWithComps? sphere) {
        if (sphere == null) {
            return false;
        }

        if (storedSpheres.Count >= props.maxCapacity) {
            // Adjust max capacity as needed
            return false;
        }

        storedSpheres.Add(sphere);
        return true;
    }

    public static SpherePouch? GetWornSpherePouch(Pawn pawn) {
        if (pawn.apparel == null) return null;

        foreach (Apparel apparel in pawn.apparel.WornApparel) {
            if (apparel.TryGetComp(out SpherePouch pouch)) return pouch;
        }

        return null; // Pawn is not wearing a Sphere Pouch
    }
}