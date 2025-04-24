using System;
using System.Linq;
using CosmereScadrial.Abilities.Hediffs;
using CosmereScadrial.Abilities.Hediffs.Comps;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Comps.Maps {
    public class PhysicalExternalAuraRenderer(Map map) : MapComponent(map) {
        public override void MapComponentUpdate() {
            foreach (var pawn in map.mapPawns.AllPawnsSpawned) {
                if (!pawn.Spawned || pawn.Dead) continue;

                var hediff = pawn.health?.hediffSet.hediffs?.OfType<AllomanticHediff>()
                    .FirstOrDefault(x => x.def.defName is "Cosmere_Hediff_SteelAura" or "Cosmere_Hediff_IronAura" && x.TryGetComp<PhysicalExternalAura>() != null);
                if (hediff == null) continue;
                var comp = hediff.TryGetComp<PhysicalExternalAura>();


                GenDraw.DrawCircleOutline(pawn.DrawPos, comp.props.radius * hediff.Severity, SimpleColor.Blue);
                foreach (var (thing, mass) in comp.thingsToDraw.Where(x => x.Value > 0f)) {
                    DrawLine(pawn, thing, mass, comp.props.lineMaterial, comp.props.radius * hediff.Severity);
                }
            }
        }

        // Fade isnt working
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