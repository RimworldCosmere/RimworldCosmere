using CosmereFramework.Extensions;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CosmereScadrial.Abilities.Allomancy;

public abstract partial class AbstractAbility {
    /*public override bool CanApplyOn(LocalTargetInfo targetInfo) {
        Pawn? targetPawn = targetInfo.Pawn;
        if (targetFlags.Has(TargetFlags.Locations) && !targetInfo.HasThing &&
            !targetInfo.TryGetPawn(out _)) {
            return true;
        }

        if (targetFlags.Has(TargetFlags.Self) && targetPawn == pawn) return true;
        if (!targetFlags.Has(TargetFlags.Self) && targetPawn == pawn) return false;
        if (targetFlags.Has(TargetFlags.Pawns) && targetPawn != null) return true;
        if (targetFlags.Has(TargetFlags.Fires) && targetPawn != null && targetPawn.IsBurning()) return true;
        if (targetFlags.Has(TargetFlags.Fires) && targetInfo.HasThing && targetInfo.Thing.IsBurning()) return true;
        if (targetFlags.Has(TargetFlags.Fires) && !targetInfo.HasThing &&
            targetInfo.Cell.ContainsStaticFire(pawn.Map)) {
            return true;
        }

        if (targetFlags.Has(TargetFlags.Buildings) && targetPawn == null && targetInfo.HasThing &&
            targetInfo.Thing.def.category == ThingCategory.Building) {
            return true;
        }

        if (targetFlags.Has(TargetFlags.Items) && targetPawn == null && targetInfo.HasThing &&
            targetInfo.Thing.def.category == ThingCategory.Item) {
            return true;
        }

        if (targetFlags.Has(TargetFlags.Animals) && targetPawn != null &&
            targetInfo.Thing.def.race.Animal) {
            return true;
        }

        if (targetFlags.Has(TargetFlags.Humans) && targetPawn != null && targetInfo.Thing.def.race.Humanlike) {
            return true;
        }

        if (targetFlags.Has(TargetFlags.Mechs) && targetPawn != null && targetInfo.Thing.def.race.IsMechanoid) {
            return true;
        }

        if (targetFlags.Has(TargetFlags.Plants) && targetPawn == null && targetInfo.HasThing &&
            targetInfo.Thing.def.plant != null) {
            return true;
        }

        if (targetFlags.Has(TargetFlags.Mutants) && targetPawn is { IsMutant: true }) return true;
        if (targetFlags.Has(TargetFlags.WorldCell) && !targetInfo.HasThing &&
            !targetInfo.TryGetPawn(out _)) {
            return true;
        }

        return false;
    }*/

    /*public virtual AcceptanceReport CanActivate(LocalTargetInfo targetInfo, BurningStatus activationStatus,
        bool ignoreInvestiture = false) {
        if (!metalBurning.CanBurn(metal, def.beuPerTick)) {
            return "CS_CannotBurn".Translate(metal.Named("METAL"));
        }

        if (activationStatus == BurningStatus.Flaring && PawnUtility.IsAsleep(pawn)) {
            return "CS_CannotFlareAsleep".Translate(pawn.Named("PAWN"), metal.Named("METAL"));
        }

        if (!ignoreInvestiture && targetInfo.Pawn != null && InvestitureDetector.IsShielded(targetInfo.Pawn)) {
            return "CS_TargetShielded".Translate(targetInfo.Pawn.Named("PAWN"));
        }

        return true;
    }*/

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