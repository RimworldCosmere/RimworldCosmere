using Cosmere.Core.Comp.Map;
using Cosmere.System.Scadrial.Def;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Allomancy.Comp.Hediff;

public class PhysicalExternalAuraProperties : LineDrawingAuraProperties {
    public PhysicalExternalAuraProperties() {
        compClass = typeof(PhysicalExternalAura);
    }

    public override Color lineColor => new Color(0.3f, 0.6f, 1f, 1f);
}

public class PhysicalExternalAura : LineDrawingAura {
    protected override IEnumerable<Verse.Thing> GetThingsToDrawInCell(IntVec3 cell, Map map) {
        List<Verse.Thing> things = cell.GetThingList(map);
        for (int i = 0; i < things.Count; i++) {
            if (things[i].GetMetalMass() > 0.0) yield return things[i];
        }
    }

    protected override LineToRender GetLineToRender(Verse.Thing thing) {
        float distance = (thing.DrawPos - parent.pawn.DrawPos).ToIntVec3().LengthHorizontal;
        float mass = thing.GetMetalMass() * thing.stackCount;

        float fade = FadeFor(distance);

        // Thickness scales between 0.15 and 0.3 based on metal mass,
        // with 10kg or more giving maximum thickness.
        float thickness = Mathf.Lerp(0.15f, 0.3f, Mathf.Clamp01(mass / 10f));

        return new LineToRender(
            metal.allomancy!.polarity == AllomancyPolarity.Pulling ? parent.pawn : thing,
            metal.allomancy.polarity == AllomancyPolarity.Pulling ? thing : parent.pawn,
            cachedLineMaterial ??= props.lineMaterial,
            fade,
            thickness
        );
    }
}
