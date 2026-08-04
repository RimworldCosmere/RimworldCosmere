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
    // The targeting preview builds its lines through here too, so a line previewed under the cursor
    // is the same line the aura draws once the metal is lit.
    public static LineToRender MetalLine(Pawn pawn, Verse.Thing thing, bool pulling, Material material, float radius) {
        float distance = (thing.DrawPos - pawn.DrawPos).ToIntVec3().LengthHorizontal;
        float mass = thing.GetMetalMass() * thing.stackCount;

        // Thickness scales between 0.15 and 0.3 based on metal mass,
        // with 10kg or more giving maximum thickness.
        float thickness = Mathf.Lerp(0.15f, 0.3f, Mathf.Clamp01(mass / 10f));

        return new LineToRender(
            pulling ? pawn : thing,
            pulling ? thing : pawn,
            material,
            LineFade.For(radius, distance),
            thickness
        );
    }

    protected override IEnumerable<Verse.Thing> GetThingsToDrawInCell(IntVec3 cell, Map map) {
        List<Verse.Thing> things = cell.GetThingList(map);
        for (int i = 0; i < things.Count; i++) {
            if (things[i].GetMetalMass() > 0.0) yield return things[i];
        }
    }

    protected override LineToRender GetLineToRender(Verse.Thing thing) {
        return MetalLine(
            parent.pawn,
            thing,
            metal.allomancy!.polarity == AllomancyPolarity.Pulling,
            cachedLineMaterial ??= props.lineMaterial,
            radius
        );
    }
}
