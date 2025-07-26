using Cosmere.Core.Comp.Thing;
using Cosmere.Framework.Comp.Thing;
using Cosmere.Resources;
using Cosmere.Resources.Def;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Comp.Thing;

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
    private IEnumerable<Verse.Thing> spheres {
        get {
            if (parent is ISlotGroupParent slotGroupParent) {
                return slotGroupParent.GetSlotGroup().HeldThings.Where(IsSphere);
            }

            if (parent is IThingHolder thingHolder) return thingHolder.GetDirectlyHeldThings().Where(IsSphere);
            if (parent.TryGetComp(out InnerStorage innerStorage)) {
                return innerStorage.GetDirectlyHeldThings().Where(IsSphere);
            }

            return IsSphere(parent) ? [parent] : [];
        }
    }

    private new SphereGlowerProperties props => (SphereGlowerProperties)base.props;

    public override float GlowRadius {
        get {
            float total = 0f;
            float max = 0f;

            foreach (Verse.Thing? sphere in spheres) {
                if (!sphere.TryGetComp(out InvestitureHolder investiture) ||
                    investiture.currentInvestiture <= 0.0) {
                    continue;
                }

                total += investiture.currentInvestiture;
                max += investiture.maxInvestiture;
            }

            if (max <= 0.0) return 0f;

            // Lerp between 0.5 and max radius based on percent fill
            float percent = Mathf.Clamp01(total / max);
            return Mathf.Lerp(props.minRadius, props.maxRadius, percent);
        }
        set { }
    }

    public override ColorInt GlowColor {
        get {
            Color blended = Color.black;
            float totalWeight = 0f;

            foreach (Verse.Thing? sphere in spheres) {
                InvestitureHolder? investiture = sphere.GetInvestiture();
                if (investiture == null || investiture.currentInvestiture <= 0f) continue;

                float weight = investiture.currentInvestiture;
                Color sphereColor = GetSphereColor(sphere);

                blended += sphereColor * weight;
                totalWeight += weight;
            }

            return totalWeight <= 0f ? new ColorInt(Color.black) : new ColorInt(blended / totalWeight);
        }
        set { }
    }

    protected override bool ShouldBeLitNow =>
        parent.Spawned && spheres.Any(s => s.GetInvestiture()?.currentInvestiture > 0);

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

        UpdateLit(parent.Map);
    }

    private static Color GetSphereColor(Verse.Thing sphere) {
        GemDef? gem = DefDatabase<GemDef>.GetNamed(
            sphere.Stuff?.defName.Replace("Raw", "") ?? Resources.ThingDefOf.RawDiamond.defName
        );

        // Replace this with however your sphere stores color — possibly via a GemstoneDef
        return gem?.glowColor ?? gem?.color ?? GemDefOf.Diamond.glowColor ?? GemDefOf.Diamond.color;
    }
}