using System;
using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Abilities.Allomancy.Hediffs;
using CosmereScadrial.Abilities.Allomancy.Hediffs.Comps;
using UnityEngine;
using Verse;
using static CosmereFramework.CosmereFramework;

namespace CosmereScadrial.Comps.Maps;

public class AuraLineRenderer(Map map) : MapComponent(map) {
    public override void MapComponentUpdate() {
        foreach (Pawn? pawn in Find.Selector.SelectedPawns) {
            if (!pawn.Spawned || pawn.Dead) continue;

            foreach (AllomanticHediff? hediff in pawn.health?.hediffSet.hediffs.OfType<AllomanticHediff>() ?? []) {
                if (!hediff.def.Equals(HediffDefOf.Cosmere_Hediff_Bronze_Aura) &&
                    !hediff.def.Equals(HediffDefOf.Cosmere_Hediff_Steel_Aura) &&
                    !hediff.def.Equals(HediffDefOf.Cosmere_Hediff_Iron_Aura)) {
                    continue;
                }

                float radius;
                Material lineMaterial;
                Dictionary<Thing, float> thingsToDraw;
                if (hediff.def.Equals(HediffDefOf.Cosmere_Hediff_Bronze_Aura)) {
                    BronzeAura? bronzeAuraComp = hediff.TryGetComp<BronzeAura>();
                    radius = bronzeAuraComp.props.radius;
                    lineMaterial = bronzeAuraComp.props.lineMaterial;
                    thingsToDraw = bronzeAuraComp.thingsToDraw;
                } else {
                    PhysicalExternalAura? physicalExternalAuraComp = hediff.TryGetComp<PhysicalExternalAura>();
                    radius = physicalExternalAuraComp.props.radius;
                    lineMaterial = physicalExternalAuraComp.props.lineMaterial;
                    thingsToDraw = physicalExternalAuraComp.thingsToDraw;
                }

                if (cosmereSettings.debugMode) {
                    GenDraw.DrawCircleOutline(pawn.DrawPos, radius * hediff.Severity, lineMaterial);
                }

                foreach ((Thing? thing, float mass) in thingsToDraw.Where(x => x.Value > 0f)) {
                    DrawLine(pawn, thing, mass, lineMaterial, radius * hediff.Severity);
                }
            }
        }
    }

    private static void DrawLine(Pawn pawn, Thing thing, float mass, Material lineMaterial, float radius) {
        float distance = (thing.Position - pawn.Position).LengthHorizontal;
        float clampedMass = Mathf.Clamp(mass, 1f, radius);
        double fade = Math.Round(1f - Mathf.Clamp01(distance / radius), 2);
        float thickness = Mathf.Lerp(0.05f, .2f, clampedMass / radius);

        MaterialPool.TryGetRequestForMat(lineMaterial, out MaterialRequest materialRequest);
        materialRequest.color.a = (float)fade;

        Vector3 thingDrawPos = thing.DrawPosHeld ?? thing.DrawPos;
        GenDraw.DrawLineBetween(
            pawn.DrawPos,
            thingDrawPos.ToIntVec3().ToVector3ShiftedWithAltitude(AltitudeLayer.Terrain),
            MaterialPool.MatFrom(materialRequest),
            thickness
        );
    }
}