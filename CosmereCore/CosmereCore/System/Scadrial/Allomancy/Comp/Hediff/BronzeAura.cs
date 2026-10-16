using Cosmere.Core.Comp.Map;
using Cosmere.Core.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Allomancy.Comp.Hediff;

public class BronzeAuraProperties : LineDrawingAuraProperties {
    public ThingDef moteDef = null!;

    public BronzeAuraProperties() {
        compClass = typeof(BronzeAura);
    }

    public override Color LineColor => MetallicArtsMetalDefOf.Bronze.color;
}

public class BronzeAura : LineDrawingAura {
    private Mote? mote;

    private float MoteScale => MoteUtility.GetMoteSize(Props.moteDef, Props.Radius, Parent.Severity);

    private new BronzeAuraProperties Props => (BronzeAuraProperties)base.Props;

    protected override IEnumerable<Verse.Thing> GetThingsToDrawInCell(IntVec3 cell, Map map) {
        List<Verse.Thing> things = cell.GetThingList(map);
        for (int i = 0; i < things.Count; i++) {
            if (things[i].GetInvestiture()?.CurrentInvestiture > 0) yield return things[i];
        }
    }

    protected override LineToRender GetLineToRender(Verse.Thing thing) {
        float distance = (thing.DrawPos - Parent.pawn.DrawPos).ToIntVec3().LengthHorizontal;
        float investiture = thing.GetInvestiture()?.CurrentInvestiture ?? 0f;

        float fade = FadeFor(distance);

        // thickness scales 0.15-1 with investiture, maxing at 10000 beu or more
        float thickness = Mathf.Lerp(0.15f, 1f, Mathf.Clamp01(investiture / 50000f));

        return new LineToRender(
            Parent.pawn,
            thing,
            cachedLineMaterial ??= Props.LineMaterial,
            fade,
            thickness
        );
    }

    public override void CompPostTick(ref float severityAdjustment) {
        CreateMote()?.Maintain();
        if (mote != null) {
            mote.Graphic.drawSize = new Vector2(MoteScale, MoteScale);
        }

        base.CompPostTick(ref severityAdjustment);

        // ThingDef
    }

    private Mote? CreateMote() {
        if (mote?.Destroyed == false) {
            return mote;
        }

        return mote ??= MoteMaker.MakeAttachedOverlay(
            Parent.pawn,
            Props.moteDef,
            Vector3.zero,
            MoteScale
        );
    }
}
