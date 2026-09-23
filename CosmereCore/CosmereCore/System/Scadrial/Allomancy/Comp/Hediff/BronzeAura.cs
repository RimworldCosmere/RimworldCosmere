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

    public override Color lineColor => MetallicArtsMetalDefOf.Bronze.color;
}

public class BronzeAura : LineDrawingAura {
    private Mote? mote;

    private float moteScale => MoteUtility.GetMoteSize(props.moteDef, props.radius, parent.Severity);

    private new BronzeAuraProperties props => (BronzeAuraProperties)base.props;

    protected override IEnumerable<Verse.Thing> GetThingsToDrawInCell(IntVec3 cell, Map map) {
        List<Verse.Thing> things = cell.GetThingList(map);
        for (int i = 0; i < things.Count; i++) {
            if (things[i].GetInvestiture()?.currentInvestiture > 0) yield return things[i];
        }
    }

    protected override LineToRender GetLineToRender(Verse.Thing thing) {
        float distance = (thing.DrawPos - parent.pawn.DrawPos).ToIntVec3().LengthHorizontal;
        float investiture = thing.GetInvestiture()?.currentInvestiture ?? 0f;

        float fade = FadeFor(distance);

        // thickness scales 0.15-1 with investiture, maxing at 10000 beu or more
        float thickness = Mathf.Lerp(0.15f, 1f, Mathf.Clamp01(investiture / 50000f));

        return new LineToRender(
            parent.pawn,
            thing,
            cachedLineMaterial ??= props.lineMaterial,
            fade,
            thickness
        );
    }

    public override void CompPostTick(ref float severityAdjustment) {
        CreateMote()?.Maintain();
        if (mote != null) {
            mote.Graphic.drawSize = new Vector2(moteScale, moteScale);
        }

        base.CompPostTick(ref severityAdjustment);

        // ThingDef
    }

    private Mote? CreateMote() {
        if (mote?.Destroyed == false) {
            return mote;
        }

        return mote ??= MoteMaker.MakeAttachedOverlay(
            parent.pawn,
            props.moteDef,
            Vector3.zero,
            moteScale
        );
    }
}
