using Cosmere.System.Scadrial.Allomancy.Hediff;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Allomancy.Ability;

public class GoldAbility : AllomancyAbility {
    private Pawn? hallucination;
    private AllomanticHediff? hediff;
    public GoldAbility(Pawn pawn) : base(pawn) { }
    public GoldAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override void AbilityTick() {
        base.AbilityTick();

        if (!atLeastBurning) {
            return;
        }

        if (pawn.CurJob?.def.Equals(JobDefOf.Cosmere_Scadrial_Job_FollowGoldHallucination) != true && hallucination != null) {
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

        if (pawn.CurJob?.def.Equals(JobDefOf.Cosmere_Scadrial_Job_FollowGoldHallucination) == true) {
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
        hallucination.gender = pawn.gender;
        hallucination.story.bodyType = pawn.story.bodyType;
        hallucination.story.headType = pawn.story.headType;
        hallucination.story.skinColorOverride = pawn.story.SkinColor;
        hallucination.story.HairColor = pawn.story.HairColor;
        hallucination.story.hairDef = pawn.story.hairDef;
        hallucination.playerSettings = null;
        hallucination.drafter = null;
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