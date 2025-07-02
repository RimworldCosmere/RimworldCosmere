using System.Collections.Generic;
using System.Linq;
using CosmereFramework.Extension;
using CosmereScadrial.Def;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Allomancy.Comp.Game;

public class GradualMoverManager(Verse.Game game) : GameComponent {
    private readonly List<MovementData> activeMovements = new List<MovementData>();

    public override void GameComponentTick() {
        for (int i = activeMovements.Count - 1; i >= 0; i--) {
            MovementData m = activeMovements[i];
            List<Pawn> haveDamaged = m.haveDamaged;
            if (m.source is not Pawn pawn) continue;
            if (m.thing.Destroyed || m.thing.Map == null || game.CurrentMap != m.thing.Map) continue;


            m.ticksElapsed++;
            float t = Mathf.Clamp01((float)m.ticksElapsed / m.ticksTotal);
            m.thing.Position = Vector3.Lerp(m.start, m.end, t).ToIntVec3();
            IEnumerable<Pawn> pawnsInSameCell = m.thing.ThingsSharingPosition<Pawn>()
                .Where(x => !x.Equals(pawn) && !haveDamaged.Contains(x) && !x.Faction.IsPlayer);
            foreach (Pawn? pawn1 in pawnsInSameCell) {
                ApplyDragDamage(pawn1, pawn);
                haveDamaged.Add(pawn1);
            }

            FleckMaker.ThrowDustPuff(m.thing.Position, m.thing.Map, 1f);
            GenDraw.DrawLineBetween(
                m.source.DrawPos,
                m.thing.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Terrain),
                m.material
            );
            if (m.ticksElapsed >= m.ticksTotal) {
                m.thing.Position = m.end.ToIntVec3(); // finalize position
                activeMovements.RemoveAt(i);
                if (m.polarity != AllomancyPolarity.Pulling || !m.thing.Position.Equals(m.source.Position)) {
                    continue;
                }

                if (!m.thing.def.EverHaulable || pawn.inventory == null) continue;

                JobDef jobDef = m.thing.CanBeEquippedBy(pawn)
                    ? RimWorld.JobDefOf.Equip
                    : RimWorld.JobDefOf.TakeCountToInventory;
                Job job = JobMaker.MakeJob(jobDef, m.thing);
                job.count = m.thing.GetMaxAmountToPickupForPawn(pawn, m.thing.stackCount);
                if (job.count < 1) break;

                pawn.jobs.TryTakeOrderedJob(job);
            } else {
                activeMovements[i] = m; // update struct
            }
        }
    }

    private void ApplyDragDamage(Verse.Thing thing, Pawn instigator) {
        if (thing is not Pawn pawn || thing == instigator) return;

        pawn.TakeDamage(
            new DamageInfo(
                DamageDefOf.Scratch,
                1f,
                instigator: instigator
            )
        );
    }

    public void StartMovement(
        AllomancyPolarity polarity,
        Verse.Thing source,
        Verse.Thing thing,
        IntVec3 destination,
        int duration,
        Material material
    ) {
        Log.Verbose(
            $"{source.LabelCap} {polarity} {thing.LabelCap} to {destination} (from {thing.Position}) for {duration} ticks"
        );
        activeMovements.Add(
            new MovementData {
                polarity = polarity,
                source = source,
                thing = thing,
                start = thing.DrawPos,
                end = destination.ToVector3Shifted(),
                material = material,
                ticksTotal = duration,
                ticksElapsed = 0,
                haveDamaged = [],
            }
        );
    }

    private struct MovementData {
        public AllomancyPolarity polarity;
        public Verse.Thing source;
        public Verse.Thing thing;
        public Material material;
        public Vector3 start;
        public Vector3 end;
        public int ticksTotal;
        public int ticksElapsed;
        public List<Pawn> haveDamaged;
    }
}