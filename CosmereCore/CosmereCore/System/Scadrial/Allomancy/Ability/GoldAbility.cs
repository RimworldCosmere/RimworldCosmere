using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Allomancy.Ability;

public class GoldAbility : AllomancyAbility {
    private GoldShadow? hallucination;
    private AllomanticHediff? hediff;

    public GoldAbility(Pawn pawn) : base(pawn) { }

    public GoldAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override void AbilityTick() {
        base.AbilityTick();

        if (!atLeastBurning) {
            return;
        }

        if (pawn.CurJob?.def.Equals(JobDefOf.Cosmere_Scadrial_Job_FollowGoldHallucination) != true &&
            hallucination != null) {
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            job = JobMaker.MakeJob(JobDefOf.Cosmere_Scadrial_Job_FollowGoldHallucination, hallucination);
            job.source = this;
            job.ability = this;
            job.verbToUse = verb;
            job.playerForced = true;
            job.targetA = hallucination;
            job.followRadius = 4f;

            pawn.jobs.TryTakeOrderedJob(job);
        }

        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond)) return;

        hediff = (AllomanticHediff?)GetOrAddHediff(pawn);
        if (hediff != null) hediff.ExtraSeverity += 0.06f;
    }

    protected override void OnEnable() {
        base.OnEnable();
        SpawnHallucination();
    }

    protected override void OnDisable() {
        base.OnDisable();
        if (hediff != null && pawn.health.hediffSet.HasHediff(hediff.def)) {
            pawn.health.RemoveHediff(hediff);
        }

        if (pawn.CurJob?.def.Equals(JobDefOf.Cosmere_Scadrial_Job_FollowGoldHallucination) == true) {
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        if (hallucination == null) return;

        if (!hallucination.Destroyed) {
            hallucination.Destroy();
        }

        hallucination = null;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref hallucination, "hallucination");
    }

    private void SpawnHallucination() {
        Map? map = pawn.MapHeld;
        if (map == null) return;

        GoldShadow shadow = (GoldShadow)IllusoryPawnUtility.Create(
            pawn,
            PawnKindDefOf.Cosmere_Scadrial_PawnKind_GoldShadow
        );
        shadow.owner = pawn;

        IntVec3 loc = CellFinder.RandomClosewalkCellNear(pawn.Position, map, 3);
        GenSpawn.Spawn(shadow, loc, map);
        shadow.Rotation = Rot4.Random;

        hallucination = shadow;
    }
}
