using System.Collections.Generic;
using CosmereScadrial.Defs;
using RimWorld;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Comps.Game {
    public class GradualMoverManager(Verse.Game game) : GameComponent {
        private readonly List<MovementData> activeMovements = new List<MovementData>();

        public override void GameComponentTick() {
            for (var i = activeMovements.Count - 1; i >= 0; i--) {
                var m = activeMovements[i];
                if (m.thing.Destroyed || m.thing.Map == null || game.CurrentMap != m.thing.Map) continue;

                m.ticksElapsed++;
                var t = Mathf.Clamp01((float)m.ticksElapsed / m.ticksTotal);
                m.thing.Position = Vector3.Lerp(m.start, m.end, t).ToIntVec3();

                FleckMaker.ThrowDustPuff(m.thing.Position, m.thing.Map, 1f);
                GenDraw.DrawLineBetween(
                    m.source.DrawPos,
                    m.thing.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Terrain),
                    m.material
                );
                if (m.ticksElapsed >= m.ticksTotal) {
                    m.thing.Position = m.end.ToIntVec3(); // finalize position
                    activeMovements.RemoveAt(i);
                    if (m.polarity != AllomancyPolarity.Pulling || !m.thing.Position.Equals(m.source.Position)) continue;
                    if (m.source is not Pawn pawn) continue;
                    if (!m.thing.def.EverHaulable || pawn.inventory == null) continue;

                    var job = JobMaker.MakeJob(RimWorld.JobDefOf.TakeCountToInventory, m.thing);
                    job.count = Mathf.Min(m.thing.stackCount, m.thing.def.stackLimit);
                    pawn.jobs.TryTakeOrderedJob(job);
                } else {
                    activeMovements[i] = m; // update struct
                }
            }
        }

        public void StartMovement(AllomancyPolarity polarity, Thing source, Thing thing, IntVec3 destination, int duration, Material material) {
            Log.Verbose($"{source.LabelCap} {polarity} {thing.LabelCap} to {destination} (from {thing.Position}) for {duration} ticks");
            activeMovements.Add(new MovementData {
                polarity = polarity,
                source = source,
                thing = thing,
                start = thing.DrawPos,
                end = destination.ToVector3Shifted(),
                material = material,
                ticksTotal = duration,
                ticksElapsed = 0,
            });
        }

        private struct MovementData {
            public AllomancyPolarity polarity;
            public Thing source;
            public Thing thing;
            public Material material;
            public Vector3 start;
            public Vector3 end;
            public int ticksTotal;
            public int ticksElapsed;
        }
    }
}