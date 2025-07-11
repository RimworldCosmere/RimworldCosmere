using System.Collections.Generic;
using System.Linq;
using CosmereCore.Extension;
using CosmereFramework.Comp.Map;
using CosmereScadrial.Def;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Allomancy.Comp.Hediff;

public class PhysicalExternalAuraProperties : LineDrawingAuraProperties {
    public PhysicalExternalAuraProperties() {
        compClass = typeof(PhysicalExternalAura);
    }

    public override Color lineColor => new Color(0.3f, 0.6f, 1f, 1f);
}

public class PhysicalExternalAura : LineDrawingAura {
    protected override IEnumerable<Verse.Thing> GetThingsToDrawInCell(IntVec3 cell, Map map) {
        return cell.GetThingList(map).Where(t => t.GetMetalMass() > 0.0);
    }

    protected override LineToRender GetLineToRender(Verse.Thing thing) {
        float distance = (thing.DrawPos - parent.pawn.DrawPos).ToIntVec3().LengthHorizontal;
        float mass = thing.GetMetalMass() * thing.stackCount;

        // Fade is fully opaque (1.0) if the object is within 3 tiles,
        // then linearly fades out to a minimum of 0.3 as it approaches the edge of the radius.
        // The fade value never goes below 0.3 to keep distant lines visible.
        float fade = Mathf.Max(0.3f, Mathf.Clamp01((radius - Mathf.Max(distance, 3f)) / (radius - 3f)));

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