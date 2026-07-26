using Cosmere.Core.Comp.Thing;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Comp.Thing;

public class StormlightBuildingGlowerProperties : CompProperties_Glower {
    public float maxRadius = 6f;
    public float minRadius = 1f;

    public StormlightBuildingGlowerProperties() {
        compClass = typeof(StormlightBuildingGlower);
    }

    public override void PostLoadSpecial(ThingDef parent) {
        glowRadius = maxRadius;
        glowColor = new ColorInt(200, 220, 255, 0);
        overlightRadius = 0f;
        colorPickerEnabled = false;
        darklightToggle = false;
    }
}

public class StormlightBuildingGlower : CompGlower {
    private new StormlightBuildingGlowerProperties props => (StormlightBuildingGlowerProperties)base.props;

    public override float GlowRadius {
        get {
            InvestitureHolder? holder = parent.GetComp<InvestitureHolder>();
            if (holder == null) return 0f;

            float current = holder.currentInvestitureSelf;
            float max = holder.maxInvestitureSelf;
            if (max <= 0f || current <= 0f) return 0f;

            float percent = Mathf.Clamp01(current / max);
            return Mathf.Lerp(props.minRadius, props.maxRadius, percent);
        }
        set { }
    }

    protected override bool ShouldBeLitNow {
        get {
            if (!parent.Spawned) return false;
            InvestitureHolder? holder = parent.GetComp<InvestitureHolder>();
            return holder != null && holder.currentInvestitureSelf > 0f;
        }
    }

    public override void ReceiveCompSignal(string signal) {
        if (parent?.Map == null) return;
        if (signal is "Cosmere_Investiture_Changed") {
            UpdateLit(parent.Map);
        }
    }
}