using System;
using Cosmere.Core.Gene;
using Cosmere.Core.Hediff;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using AbilityDef = Cosmere.Core.Def.AbilityDef;
using Logger = Cosmere.Framework.Logger;

namespace Cosmere.Core.Ability;

public interface IAbility<out TGene, out THediff> where TGene : Invested where THediff : IHediff<TGene> {
    public TGene gene { get; }
    public AbilityDef def { get; set; }
    public void UpdateStatus(Status? nextStatus = null);
    public float GetStrength(Status? nextStatus = null);
    public event Action<IAbility<TGene, THediff>, Status, Status>? OnStatusChangedEvent;
}

public abstract class AbstractAbility(Pawn pawn, RimWorld.AbilityDef def)
    : AbstractAbility<Invested, AbstractHediff<Invested>>(pawn, def);

public abstract class AbstractAbility<TGene>(Pawn pawn, RimWorld.AbilityDef def)
    : AbstractAbility<TGene, AbstractHediff<TGene>>(pawn, def) where TGene : Invested;

public abstract class AbstractAbility<TGene, THediff> : RimWorld.Ability, IAbility<TGene, THediff>
    where TGene : Invested where THediff : IHediff<TGene> {
    protected Mote? activeMote;
    protected TGene? cachedGene;

    public GlobalTargetInfo? globalTarget;
    protected Job? job;
    public LocalTargetInfo? localTarget;
    public bool paused;
    public Status status = Active.Off;
    public bool willUseWhileDowned;
    public bool willUseWhileInjured;

    public AbstractAbility(Pawn pawn, RimWorld.AbilityDef def) : base(pawn, def) {
        Initialize();
    }

    protected virtual bool toggleable => true;

    public Status? nextStatus { get; protected set; }

    public override AcceptanceReport CanCast => gene.CanLowerReserve(def.beuPerTick);

    public override string Tooltip {
        get {
            List<string> tooltipByLine = base.Tooltip.Split('\n').ToList();
            if (willUseWhileDowned) {
                tooltipByLine.Insert(1, "CC_WillUseWhileDowned".Translate().Colorize(ColorLibrary.Green));
            }

            if (willUseWhileInjured) {
                tooltipByLine.Insert(1, "CC_WillUseWhileInjured".Translate().Colorize(ColorLibrary.Green));
            }

            return string.Join("\n", tooltipByLine.ToArray());
        }
    }

    public new AbilityDef def {
        get => (AbilityDef)base.def;
        set => base.def = value;
    }

    public virtual TGene gene { get; }

    public event Action<IAbility<TGene, THediff>, Status, Status>? OnStatusChangedEvent;

    public virtual float GetStrength(Status? desiredStatus = null) {
        float baseStrength = (desiredStatus ?? status).power * def.hediffSeverityFactor;

        if (pawn.IsAsleep()) {
            return baseStrength * def.asleepStrengthFactor;
        }

        if (pawn.Downed) {
            return baseStrength * def.downedStrengthFactor;
        }

        return baseStrength;
    }

    public void UpdateStatus(Status? newStatus = null) {
        if (newStatus == null) {
            if (nextStatus == null) {
                Logger.Error("Call to UpdateStatus with no status, and no next status");
                return;
            }

            newStatus = nextStatus;
        }

        if (status == newStatus) {
            return;
        }

        Status? oldStatus = status;
        status = newStatus.Value;

        if (oldStatus == false && def.activeMote != null) {
            activeMote ??= MoteMaker.MakeAttachedOverlay(pawn, def.activeMote, Vector3.zero, GetMoteScale());
        } else if (newStatus == false) {
            activeMote?.Destroy();
            activeMote = null;
        }

        if (activeMote != null) {
            activeMote.Scale = GetMoteScale();
        }

        OnStatusChanged(oldStatus.Value, newStatus.Value);


        nextStatus = null;
    }

    public new virtual void Initialize() {
        if (def.comps.Any<AbilityCompProperties>()) {
            comps = [];
            for (int index = 0; index < def.comps.Count; ++index) {
                AbilityComp? abilityComp = null;
                try {
                    abilityComp = (AbilityComp)Activator.CreateInstance(def.comps[index].compClass);
                    abilityComp.parent = this;
                    comps.Add(abilityComp);
                    abilityComp.Initialize(def.comps[index]);
                } catch (Exception ex) {
                    Log.Error("Could not instantiate or initialize an AbilityComp: " + ex);
                    comps.Remove(abilityComp);
                }
            }
        }

        if (Id == -1) {
            Id = Find.UniqueIDsManager.GetNextAbilityID();
        }

        if (VerbTracker.PrimaryVerb is IAbilityVerb primaryVerb) {
            primaryVerb.Ability = this;
        }

        willUseWhileDowned = def is { canUseWhileDowned: true, autoUseWhileDowned: true };
        willUseWhileInjured = def is { autoUseWhileInjured: true };

        if (def.charges <= 0) {
            return;
        }

        maxCharges = def.charges;
        RemainingCharges = maxCharges;
    }

    public override void AbilityTick() {
        base.AbilityTick();

        activeMote?.Maintain();
        HandleAutoBurn();
    }

    protected virtual void HandleAutoBurn() {
        if (willUseWhileDowned && pawn.Downed && !pawn.Dead && status.isActive) {
            if (gene.CanLowerReserve(def.beuPerTick)) {
                UpdateStatus(Active.On);
            }
        }

        if (willUseWhileInjured && gene.CanLowerReserve(def.beuPerTick)) {
            if (pawn.health.summaryHealth.SummaryHealthPercent < 1) UpdateStatus(Active.On);
            if (pawn.health.hediffSet.hediffs.Count(h => h.CanBeHealedByInvestiture()) > 0) {
                UpdateStatus(Active.On);
            }
        }

        if (!status.isActive) {
            return;
        }

        if (pawn.IsAsleep()) {
            if (!def.canUseWhileAsleep) {
                UpdateStatus(Active.Off);
            } else if (status.isPoweredUp) {
                UpdateStatus(Active.On);
            }
        } else {
            if (paused) {
                paused = false;
                gene.UpdateDrainSource((def, GetDesiredBurnRateForStatus(status)));
                OnEnable();
                if (status.isPoweredUp) OnPowerUp();
                return;
            }
        }


        if (pawn.Downed) {
            if (status.isPoweredUp && (!def.canUseWhileDowned || !willUseWhileDowned)) {
                UpdateStatus(Active.Off);
            } else if (status == Active.Off && willUseWhileDowned) {
                UpdateStatus(Active.On);
            }
        }

        if (!paused && pawn.IsAsleep() && status.isActive && !def.canUseWhileAsleep) {
            paused = true;
            gene.UpdateDrainSource((def, GetDesiredBurnRateForStatus(Active.Off)));
            OnDisable();
        }
    }

    protected virtual float GetMoteScale() {
        return status.power;
    }

    public virtual float GetDesiredBurnRateForStatus() {
        return GetDesiredBurnRateForStatus(status);
    }

    public virtual float GetDesiredBurnRateForStatus(Status? desiredStatus) {
        return (desiredStatus ?? status).power * def.beuPerTick;
    }

    public void SetNextStatus(Status desiredStatus, bool overrideNextStatus = false) {
        if (nextStatus != null && !overrideNextStatus) return;

        desiredStatus.power = Math.Clamp(desiredStatus.power, 0, def.maxPower);

        nextStatus = desiredStatus;
    }

    protected virtual void OnStatusChanged(Status oldStatus, Status newStatus) {
        gene.UpdateDrainSource((def, GetDesiredBurnRateForStatus(newStatus)));
        // OnStatusChanged needs to be called in these orders so that the SeverityCalculator can be updated properly

        // When Disabling (and possibly deflaring)
        if (status.active == Active.Off && oldStatus.active != Active.Off) {
            OnDisable();
            Cleanup();
        }

        // When enabling 
        if (status.active == Active.On && oldStatus.active == Active.Off) {
            OnEnable();
        }

        // When Flaring
        if (status.power > oldStatus.power) {
            OnPowerUp();
        }

        // When de-flaring to burning
        if (status.power < oldStatus.power) {
            OnPowerDown();
        }

        OnStatusChangedEvent?.Invoke(this, oldStatus, newStatus);
    }

    public override bool GizmoDisabled(out string reason) {
        if (!status) return base.GizmoDisabled(out reason);

        reason = "";
        return false;
    }

    /// <summary>
    ///     We hide all gizmos for this ability because they are added by the gene
    /// </summary>
    /// <returns></returns>
    public override IEnumerable<Command> GetGizmos() {
        yield break;
    }

    protected virtual void OnEnable() {
        GetOrAddHediff(localTarget.HasValue ? localTarget.Value.Pawn : pawn);
    }

    protected virtual void OnDisable() {
        nextStatus = null;
        OnPowerDown();
        if (localTarget == pawn) {
            RemoveHediff(pawn);
        }
    }

    protected virtual void Cleanup() {
        if (job != null) {
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            job = null;
        }

        localTarget = null;
        globalTarget = null;
    }

    protected virtual void OnPowerUp() {
        OnEnable();
    }

    protected virtual void OnPowerDown() { }

    protected AbstractHediff<TGene>? GetOrAddHediff(Pawn targetPawn) {
        return targetPawn.GetOrAddHediff(pawn, (IAbility<TGene, IHediff<TGene>>?)this, def);
    }

    protected void RemoveHediff(Pawn? targetPawn) {
        if (targetPawn == null) return;

        pawn.RemoveHediff(pawn, this as AbstractAbility<TGene, AbstractHediff<TGene>>, def);
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Values.Look(ref status, "status");

        switch (Scribe.mode) {
            // Save/load logic
            case LoadSaveMode.Saving: {
                bool localTargetPresent = localTarget.HasValue;
                Scribe_Values.Look(ref localTargetPresent, "targetPresent");
                if (localTargetPresent) {
                    LocalTargetInfo temp = localTarget!.Value;
                    Scribe_TargetInfo.Look(ref temp, "target");
                }

                break;
            }
            case LoadSaveMode.LoadingVars: {
                bool localTargetPresent = false;
                Scribe_Values.Look(ref localTargetPresent, "targetPresent");
                if (localTargetPresent) {
                    LocalTargetInfo temp = LocalTargetInfo.Invalid;
                    Scribe_TargetInfo.Look(ref temp, "target");
                    localTarget = temp;
                } else {
                    localTarget = null;
                }

                break;
            }
        }
    }


    public override void QueueCastingJob(LocalTargetInfo targetInfo, LocalTargetInfo dest) {
        QueueCastingJob(targetInfo, dest, 1);
    }

    public override void QueueCastingJob(GlobalTargetInfo targetInfo) {
        QueueCastingJob(targetInfo, 1);
    }

    public void QueueCastingJob(LocalTargetInfo targetInfo, LocalTargetInfo destination, int power) {
        localTarget = targetInfo;
        SetNextStatus(power);

        base.QueueCastingJob(targetInfo, destination);
    }

    public void QueueCastingJob(GlobalTargetInfo targetInfo, int power) {
        globalTarget = targetInfo;
        SetNextStatus(power);

        base.QueueCastingJob(targetInfo);
    }

    public override Job GetJob(LocalTargetInfo targetInfo, LocalTargetInfo destination) {
        if (nextStatus != null) {
            UpdateStatus(nextStatus.Value);
        }

        Job? job = JobMaker.MakeJob(def.jobDef ?? JobDefOf.CastAbilityOnThing, targetInfo);
        job.source = this;
        job.ability = this;
        job.verbToUse = verb;
        job.playerForced = true;
        job.targetA = targetInfo;
        job.targetB = destination;
        job.count = status.power;
        job.followRadius = def.verbProperties.range / 2f;

        return job;
    }

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