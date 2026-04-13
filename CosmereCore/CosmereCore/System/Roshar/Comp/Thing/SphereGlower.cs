using Cosmere.Core;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Def;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Comp.Thing;

public class SphereGlowerProperties : CompProperties_Glower {
    public float maxRadius = 9f;
    public float minRadius = 3f;

    public SphereGlowerProperties() {
        compClass = typeof(SphereGlower);
    }

    public override void PostLoadSpecial(ThingDef parent) {
        glowRadius = maxRadius;
        overlightRadius = 0f;
        colorPickerEnabled = false;
        darklightToggle = false;
        overrideIsCavePlant = false;
    }
}

public class SphereGlower : CompGlower {
    private bool FlickedOn = true;
    private List<Verse.Thing> cachedSpheres = [];
    private bool spheresCacheDirty = true;

    private List<Verse.Thing> spheres {
        get {
            if (!spheresCacheDirty) return cachedSpheres;
            cachedSpheres.Clear();
            RebuildSphereCache();
            spheresCacheDirty = false;
            return cachedSpheres;
        }
    }

    private void RebuildSphereCache() {
        if (parent is ISlotGroupParent slotGroupParent) {
            SlotGroup? slotGroup = slotGroupParent.GetSlotGroup();
            if (slotGroup != null) {
                foreach (Verse.Thing thing in slotGroup.HeldThings) {
                    if (IsSphere(thing)) cachedSpheres.Add(thing);
                }
            }
            return;
        }

        if (parent is IThingHolder thingHolder) {
            ThingOwner things = thingHolder.GetDirectlyHeldThings();
            for (int i = 0; i < things.Count; i++) {
                if (IsSphere(things[i])) cachedSpheres.Add(things[i]);
            }
            return;
        }

        if (parent.TryGetComp(out InnerStorage innerStorage)) {
            ThingOwner things = innerStorage.GetDirectlyHeldThings();
            for (int i = 0; i < things.Count; i++) {
                if (IsSphere(things[i])) cachedSpheres.Add(things[i]);
            }
            return;
        }

        if (IsSphere(parent)) cachedSpheres.Add(parent);
    }

    private new SphereGlowerProperties props => (SphereGlowerProperties)base.props;

    public override float GlowRadius {
        get {
            float total = 0f;
            float max = 0f;
            List<Verse.Thing> currentSpheres = spheres;

            for (int i = 0; i < currentSpheres.Count; i++) {
                if (!currentSpheres[i].TryGetComp(out InvestitureHolder investiture) ||
                    investiture.currentInvestiture <= 0.0) {
                    continue;
                }

                total += investiture.currentInvestiture;
                max += investiture.maxInvestiture;
            }

            if (max <= 0.0) return 0f;

            float percent = Mathf.Clamp01(total / max);
            return Mathf.Lerp(props.minRadius, props.maxRadius, percent);
        }
        set { }
    }

    public override ColorInt GlowColor {
        get {
            Color blended = Color.black;
            float totalWeight = 0f;
            List<Verse.Thing> currentSpheres = spheres;

            for (int i = 0; i < currentSpheres.Count; i++) {
                InvestitureHolder? investiture = currentSpheres[i].GetInvestiture();
                if (investiture == null || investiture.currentInvestiture <= 0f) continue;

                float weight = investiture.currentInvestiture;
                Color sphereColor = GetSphereColor(currentSpheres[i]);

                blended += sphereColor * weight;
                totalWeight += weight;
            }

            return totalWeight <= 0f ? new ColorInt(Color.black) : new ColorInt(blended / totalWeight);
        }
        set { }
    }

    protected override bool ShouldBeLitNow {
        get {
            if (!parent.Spawned || !FlickedOn) return false;
            List<Verse.Thing> currentSpheres = spheres;
            for (int i = 0; i < currentSpheres.Count; i++) {
                InvestitureHolder? inv = currentSpheres[i].GetInvestiture();
                if (inv != null && inv.currentInvestiture > 0) return true;
            }
            return false;
        }
    }

    private static bool IsSphere(Verse.Thing thing) {
        return thing.def.IsOneOf(
            ThingDefOf.Cosmere_Roshar_Thing_Broam,
            ThingDefOf.Cosmere_Roshar_Thing_Mark,
            ThingDefOf.Cosmere_Roshar_Thing_Chip
        );
    }

    public override void ReceiveCompSignal(string signal) {
        if (parent?.Map == null) return;
        if (signal is not ("FlickedOn"
            or "FlickedOff"
            or "PowerTurnedOn"
            or "PowerTurnedOff"
            or "Cosmere_Investiture_Changed"
            or "Cosmere_InnerStorage_Changed"
            or "ScheduledOn"
            or "ScheduledOff")) {
            return;
        }

        FlickedOn = signal switch {
            "FlickedOff" => false,
            "FlickedOn" => true,
            _ => FlickedOn,
        };

        spheresCacheDirty = true;
        UpdateLit(parent.Map);
    }

    private static Color GetSphereColor(Verse.Thing sphere) {
        string stuffName = sphere.Stuff?.defName.Replace("Raw", "") ?? "Diamond";
        GemDef? gem = DefDatabase<GemDef>.GetNamedSilentFail(stuffName) ?? GemDefOf.Diamond;

        return gem.glowColor ?? gem.color;
    }
}