using System.Collections.Generic;
using System.Linq;
using CosmereCore.Extension;
using CosmereFramework.Comp.Map;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Allomancy.Comp.Hediff;

public class BronzeAuraProperties : LineDrawingAuraProperties {
    public BronzeAuraProperties() {
        compClass = typeof(BronzeAura);
    }

    public override Color lineColor => MetallicArtsMetalDefOf.Bronze.color;
}

public class BronzeAura : LineDrawingAura {
    protected override IEnumerable<Verse.Thing> GetThingsToDrawInCell(IntVec3 cell, Map map) {
        return cell.GetThingList(map).Where(t => t.GetInvestiture() > 0);
    }

    protected override LineToRender GetLineToRender(Verse.Thing thing) {
        float distance = (thing.DrawPos - parent.pawn.DrawPos).ToIntVec3().LengthHorizontal;
        float investiture = thing.GetInvestiture();

        // Fade is fully opaque (1.0) if the object is within 3 tiles,
        // then linearly fades out to a minimum of 0.3 as it approaches the edge of the radius.
        // The fade value never goes below 0.3 to keep distant lines visible.
        float fade = Mathf.Max(0.3f, Mathf.Clamp01((radius - Mathf.Max(distance, 3f)) / (radius - 3f)));

        // Thickness scales between 0.15 and 1 based on thing's investiture, 
        // with 10000 BEUs or more giving maximum thickness.
        float thickness = Mathf.Lerp(0.15f, 1f, Mathf.Clamp01(investiture / 50000f));

        return new LineToRender(
            parent.pawn,
            thing,
            cachedLineMaterial ??= props.lineMaterial,
            fade,
            thickness
        );
    }
}