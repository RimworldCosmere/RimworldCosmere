using System;
using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Abilities.Allomancy.Hediffs;
using CosmereScadrial.Abilities.Allomancy.Hediffs.Comps;
using UnityEngine;
using Verse;
using static CosmereFramework.CosmereFramework;

namespace CosmereScadrial.Comps.Maps {
    public class AuraLineRenderer(Map map) : MapComponent(map) {
        public override void MapComponentUpdate() {
            foreach (var pawn in Find.Selector.SelectedPawns) {
                if (!pawn.Spawned || pawn.Dead) continue;

                foreach (var hediff in pawn.health?.hediffSet.hediffs.OfType<AllomanticHediff>() ?? []) {
                    if (!hediff.def.Equals(HediffDefOf.Cosmere_Hediff_BronzeAura) &&
                        !hediff.def.Equals(HediffDefOf.Cosmere_Hediff_SteelAura) &&
                        !hediff.def.Equals(HediffDefOf.Cosmere_Hediff_IronAura)) {
                        continue;
                    }

                    float radius;
                    Material lineMaterial;
                    Dictionary<Thing, float> thingsToDraw;
                    if (hediff.def.Equals(HediffDefOf.Cosmere_Hediff_BronzeAura)) {
                        var bronzeAuraComp = hediff.TryGetComp<BronzeAura>();
                        radius = bronzeAuraComp.props.radius;
                        lineMaterial = bronzeAuraComp.props.lineMaterial;
                        thingsToDraw = bronzeAuraComp.thingsToDraw;
                    } else {
                        var physicalExternalAuraComp = hediff.TryGetComp<PhysicalExternalAura>();
                        radius = physicalExternalAuraComp.props.radius;
                        lineMaterial = physicalExternalAuraComp.props.lineMaterial;
                        thingsToDraw = physicalExternalAuraComp.thingsToDraw;
                    }

                    if (CosmereSettings.debugMode) {
                        GenDraw.DrawCircleOutline(pawn.DrawPos, radius * hediff.Severity, SimpleColor.Blue);
                    }

                    foreach (var (thing, mass) in thingsToDraw.Where(x => x.Value > 0f)) {
                        DrawLine(pawn, thing, mass, lineMaterial, radius * hediff.Severity);
                    }
                }
            }
        }

        private static void DrawLine(Pawn pawn, Thing thing, float mass, Material lineMaterial, float radius) {
            var distance = (thing.Position - pawn.Position).LengthHorizontal;
            var clampedMass = Mathf.Clamp(mass, 1f, radius);
            var fade = Math.Round(1f - Mathf.Clamp01(distance / radius), 2);
            var thickness = Mathf.Lerp(0.05f, .2f, clampedMass / radius);

            MaterialPool.TryGetRequestForMat(lineMaterial, out var materialRequest);
            materialRequest.color.a = (float)fade;

            var thingDrawPos = thing.DrawPosHeld ?? thing.DrawPos;
            GenDraw.DrawLineBetween(
                pawn.DrawPos,
                thingDrawPos.ToIntVec3().ToVector3ShiftedWithAltitude(AltitudeLayer.Terrain),
                MaterialPool.MatFrom(materialRequest),
                thickness
            );
        }
    }
}