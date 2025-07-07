using CosmereFramework.Extension;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereScadrial.Allomancy.Ability;

public class AbilityOtherTarget : AbstractAbility {
    private Job? job;

    public AbilityOtherTarget(Pawn pawn) : base(pawn) { }

    public AbilityOtherTarget(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }

    public AbilityOtherTarget(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public AbilityOtherTarget(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

    public override void AbilityTick() {
        base.AbilityTick();

        if (!atLeastPassive) {
            return;
        }

        if (pawn.IsAsleep()) {
            UpdateStatus(BurningStatus.Off);
        }
    }

    public override Job GetJob(LocalTargetInfo targetInfo, LocalTargetInfo destination) {
        job = base.GetJob(targetInfo, destination);
        job.followRadius = def.verbProperties.range / 2f;

        return job;
    }

    protected override void OnDisable() {
        RemoveHediff(localTarget?.Pawn);
        ApplyDrag(def.applyDragOnTarget ? localTarget?.Pawn : pawn, flareDuration / 3000f);
        localTarget = null;

        if (job != null) {
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            job = null;
        }

        flareStartTick = -1;
    }

    protected override void OnFlare() {
        flareStartTick = Find.TickManager.TicksGame;
        RemoveDrag(def.applyDragOnTarget ? localTarget?.Pawn : pawn);
    }

    protected override void OnDeFlare() {
        ApplyDrag(def.applyDragOnTarget ? localTarget?.Pawn : pawn, flareDuration / 3000f / 2);
        flareStartTick = -1;
    }
}