using Cosmere.Core.Comp.Game;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Comp.Thing;

public class SprenGlowerProperties : CompProperties {
    public float baseGlowRadius = 1f;
    public ColorInt glowColor = new ColorInt(217, 217, 230);

    public SprenGlowerProperties() {
        compClass = typeof(SprenGlower);
    }
}

public class SprenGlower : ThingComp {
    private float currentRadius;
    private CompGlower? glower;

    private SprenGlowerProperties Props => (SprenGlowerProperties)props;

    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);
        glower = parent.TryGetComp<CompGlower>();
        currentRadius = Props.baseGlowRadius;
    }

    public override void CompTick() {
        base.CompTick();
        if (glower == null) return;
        if (!parent.IsHashIntervalTick(500)) return;

        SprenBond? bond = parent.TryGetComp<SprenBond>();
        if (bond?.BondedRadiant == null) return;

        float connection = SpiritWeb.Instance?.GetConnectionValue(bond.BondedRadiant, parent) ?? 0f;
        float targetRadius = Mathf.Lerp(0.2f, Props.baseGlowRadius, connection);

        if (Mathf.Abs(currentRadius - targetRadius) < 0.05f) return;

        currentRadius = targetRadius;

        Verse.Map? map = parent.Map;
        if (map == null) return;

        map.glowGrid.DeRegisterGlower(glower);
        glower.Props.glowRadius = currentRadius;
        map.glowGrid.RegisterGlower(glower);
    }
}