using Cosmere.System.Scadrial.Def;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Allomancy.Comp.Game;

public class GradualMoverManager(Verse.Game game) : GameComponent {
    private readonly List<MovementData> activeMovements = [];

    public override void GameComponentTick() {
        for (int i = activeMovements.Count - 1; i >= 0; i--) {
            MovementData movement = activeMovements[i];
            if (movement.thing.Destroyed || movement.thing.Map == null || game.CurrentMap != movement.thing.Map) {
                activeMovements.RemoveAt(i);
                continue;
            }

            Pawn? pawn = movement.source as Pawn ?? movement.thing as Pawn;
            if (pawn == null) continue;
            TickMovement(ref movement, pawn);
            TickRendering(movement);

            if (movement.ticksElapsed >= movement.ticksTotal) {
                movement.thing.Position = movement.end.ToIntVec3();
                activeMovements.RemoveAt(i);
                DispatchPickupJob(movement, pawn);
            }
            else {
                activeMovements[i] = movement;
            }
        }
    }

    private static void TickMovement(ref MovementData m, Pawn pawn) {
        m.ticksElapsed++;
        float t = Mathf.Clamp01((float)m.ticksElapsed / m.ticksTotal);
        float easedPosition = EasingFunctions.EaseInOutQuint(Mathf.Clamp01(1f - t));
        m.thing.Position = Vector3.Lerp(m.end, m.start, easedPosition).ToIntVec3();

        foreach (Pawn pawn1 in m.thing.ThingsSharingPosition<Pawn>()) {
            if (pawn1.Equals(m.source) || pawn1.Equals(m.thing) || m.haveDamaged.Contains(pawn1)) continue;
            if (pawn1.Faction?.IsPlayer == true) continue;
            ApplyDragDamage(pawn1, pawn);
            m.haveDamaged.Add(pawn1);
        }
    }

    private static void TickRendering(MovementData m) {
        FleckMaker.ThrowDustPuff(m.thing.Position, m.thing.Map, 1f);
        GenDraw.DrawLineBetween(
            m.source.DrawPos,
            m.thing.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Terrain),
            m.material
        );
    }

    private static void DispatchPickupJob(MovementData m, Pawn pawn) {
        if (m.polarity != AllomancyPolarity.Pulling || !m.thing.Position.Equals(m.source.Position)) return;
        if (!m.thing.def.EverHaulable || pawn.inventory == null) return;

        JobDef jobDef = m.thing.CanBeEquippedBy(pawn)
            ? RimWorld.JobDefOf.Equip
            : RimWorld.JobDefOf.TakeCountToInventory;
        Job job = JobMaker.MakeJob(jobDef, m.thing);
        job.count = m.thing.GetMaxAmountToPickupForPawn(pawn, m.thing.stackCount);
        if (job.count < 1) return;

        pawn.jobs.TryTakeOrderedJob(job);
    }

    private static void ApplyDragDamage(Verse.Thing thing, Verse.Thing instigator) {
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
        Logger.Verbose(
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
        public HashSet<Pawn> haveDamaged;
    }
}
