using CosmereFramework.Extension;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CosmereScadrial.Allomancy.Ability;

public abstract partial class AbstractAbility {
    public override void QueueCastingJob(LocalTargetInfo targetInfo, LocalTargetInfo dest) {
        QueueCastingJob(targetInfo, dest, false);
    }

    public override void QueueCastingJob(GlobalTargetInfo targetInfo) {
        QueueCastingJob(targetInfo, false);
    }

    public void QueueCastingJob(LocalTargetInfo targetInfo, LocalTargetInfo destination, bool flare) {
        localTarget = targetInfo;
        SetNextStatus(def.canFlare && flare ? BurningStatus.Flaring : BurningStatus.Burning);

        base.QueueCastingJob(targetInfo, destination);
    }

    public void QueueCastingJob(GlobalTargetInfo targetInfo, bool flare) {
        globalTarget = targetInfo;
        SetNextStatus(def.canFlare && flare ? BurningStatus.Flaring : BurningStatus.Burning);

        base.QueueCastingJob(targetInfo);
    }

    public override Job GetJob(LocalTargetInfo targetInfo, LocalTargetInfo destination) {
        if (nextStatus != null) {
            UpdateStatus(nextStatus.Value);
        }

        if (status == null) {
            Log.Warning("Ability doesn't have a status or a nextStatus");
        }

        Job? job = JobMaker.MakeJob(def.jobDef ?? RimWorld.JobDefOf.CastAbilityOnThing, targetInfo);
        job.ability = this;
        job.verbToUse = verb;
        job.playerForced = true;
        job.targetA = targetInfo;
        job.targetB = destination;
        job.source = this;
        job.count = (int)(status ?? BurningStatus.Burning);

        return job;
    }

    public void SetNextStatus(BurningStatus desiredStatus, bool overrideNextStatus = false) {
        if (nextStatus != null && !overrideNextStatus) return;

        if (!def.canFlare && desiredStatus is BurningStatus.Flaring or BurningStatus.Duralumin) {
            desiredStatus = BurningStatus.Burning;
        }

        if (pawn.IsAsleep() && desiredStatus >= BurningStatus.Burning) {
            desiredStatus = def.canBurnWhileAsleep ? BurningStatus.Passive : BurningStatus.Off;
        }

        if (pawn.Downed && !willBurnWhileDowned) {
            desiredStatus = BurningStatus.Off;
        }

        nextStatus = desiredStatus;
    }

    /**
     * @TODO Implement AbstractAbility.NextStatus, and read/use it here
     */
    protected override void PreActivate(LocalTargetInfo? target) {
        UpdateStatus();

        base.PreActivate(target);
    }

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        bool result = base.Activate(target, dest);
        nextStatus = null;

        return result;
    }
}