using CosmereFramework.Extensions;
using CosmereScadrial.Abilities.Allomancy.Hediffs;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereScadrial.Abilities.Allomancy;

public class GoldAbility : AbilitySelfTarget {
    private Pawn? hallucination;

    public AllomanticHediff? hediff;
    private Job? job;

    public GoldAbility() { }

    public GoldAbility(Pawn pawn) : base(pawn) { }

    public GoldAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }

    public GoldAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public GoldAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

    public override void AbilityTick() {
        base.AbilityTick();

        if (!atLeastPassive) {
            return;
        }

        if (pawn.IsAsleep() && status > BurningStatus.Passive) {
            UpdateStatus(BurningStatus.Passive);
        } else if (!pawn.IsAsleep() && status == BurningStatus.Passive) {
            UpdateStatus(BurningStatus.Burning);
        }

        if (!pawn.CurJob.def.Equals(JobDefOf.Cosmere_Scadrial_Job_FollowGoldHallucination) && hallucination != null) {
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            job = JobMaker.MakeJob(JobDefOf.Cosmere_Scadrial_Job_FollowGoldHallucination, hallucination);
            job.ability = this;
            job.verbToUse = verb;
            job.playerForced = true;
            job.targetA = hallucination;
            job.followRadius = 4f;

            pawn.jobs.TryTakeOrderedJob(job);
        }

        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond)) return;

        hediff = GetOrAddHediff(pawn);
        if (hediff != null) hediff.extraSeverity += 0.06f;
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

        if (pawn.CurJob.def.Equals(JobDefOf.Cosmere_Scadrial_Job_FollowGoldHallucination)) {
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        // Remove hallucination
        if (hallucination is { Spawned: true }) {
            hallucination.Destroy();
        }
    }

    private void SpawnHallucination() {
        Map? map = pawn.MapHeld;
        if (map == null) return;

        hallucination = PawnGenerator.GeneratePawn(
            pawn.kindDef,
            Faction.OfAncients // or a custom invisible/neutral faction
        );

        hallucination.Name = pawn.Name;
        hallucination.genes = pawn.genes;
        hallucination.gender = pawn.gender;
        hallucination.relations = pawn.relations;
        hallucination.story = pawn.story;
        hallucination.playerSettings = null;
        hallucination.drafter = null;
        hallucination.RaceProps.makesFootprints = false;
        hallucination.Rotation = Rot4.Random;

        IntVec3 loc = CellFinder.RandomClosewalkCellNear(pawn.Position, map, 3);
        GenSpawn.Spawn(hallucination, loc, map);

        // Make it non-interactive
        hallucination.health.forceDowned = true;
        hallucination.health.capacities.Clear();
        hallucination.playerSettings = null;

        hallucination.mindState.mentalStateHandler.TryStartMentalState(
            MentalStateDefOf.Roaming,
            "Hallucination revealing a different path",
            true
        );
    }
}