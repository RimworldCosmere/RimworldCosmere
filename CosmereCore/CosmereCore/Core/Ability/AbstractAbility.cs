using System;
using Cosmere.Core.Gene;
using Cosmere.Core.Hediff;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Def_AbilityDef = Cosmere.Core.Def.AbilityDef;

namespace Cosmere.Core.Ability;

public interface IAbility<out TGene, out THediff> : ILoadReferenceable
    where TGene : Invested
    where THediff : IHediff<TGene> {
    public TGene Gene { get; }

    public Def_AbilityDef def { get; }

    public void UpdateStatus(Status? newStatus = null);

    public float GetStrength(Status? desiredStatus = null);

    public event Action<IAbility<TGene, THediff>, Status, Status>? OnStatusChangedEvent;
}

public abstract class AbstractAbility(Pawn pawn, AbilityDef def)
    : AbstractAbility<Invested, AbstractHediff<Invested>>(pawn, def);

public abstract class AbstractAbility<TGene>(Pawn pawn, AbilityDef def)
    : AbstractAbility<TGene, AbstractHediff<TGene>>(pawn, def)
    where TGene : Invested;

public abstract class AbstractAbility<TGene, THediff> : RimWorld.Ability, IAbility<TGene, THediff>
    where TGene : Invested
    where THediff : IHediff<TGene> {
    protected Mote? activeMote;
    protected TGene? cachedGene;

    public GlobalTargetInfo? globalTarget;
    protected Job? job;
    private bool lastWasAsleep;

    private bool lastWasDowned;
    private bool lastWasPaused;
    public LocalTargetInfo? localTarget;
    public bool paused;
    public Status status = Active.Off;
    public bool willUseWhileDowned;
    public bool willUseWhileInjured;

    public AbstractAbility(Pawn pawn) : base(pawn) { }

    public AbstractAbility(Pawn pawn, AbilityDef def) : base(pawn, def) {
        Initialize();
    }

    public Status? nextStatus { get; protected set; }

    public override AcceptanceReport CanCast => Gene.CanLowerReserve(def.beuPerTick);

    public override string Tooltip {
        get {
            List<string> tooltipByLine = base.Tooltip.Split('\n').ToList();

            if (def.beuPerTick > 0) {
                if (def.toggleable) {
                    float drainPerSecond = GetDesiredBurnRateForStatus(Active.On) * GenTicks.TicksPerRealSecond;
                    tooltipByLine.Insert(1, $"Investiture: {drainPerSecond:F2}/s".Colorize(ColorLibrary.Cyan));
                } else {
                    float cost = GetDesiredBurnRateForStatus(Active.On);
                    tooltipByLine.Insert(1, $"Investiture: {cost:F2} per use".Colorize(ColorLibrary.Cyan));
                }
            }

            if (willUseWhileDowned) {
                tooltipByLine.Insert(2, "CC_WillUseWhileDowned".Translate().Colorize(ColorLibrary.Green));
            }

            if (willUseWhileInjured) {
                tooltipByLine.Insert(2, "CC_WillUseWhileInjured".Translate().Colorize(ColorLibrary.Green));
            }

            return string.Join("\n", tooltipByLine.ToArray());
        }
    }

    public new Def_AbilityDef def {
        get => (Def_AbilityDef)base.def;
        set => base.def = value;
    }

    public abstract TGene Gene { get; }

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

        if (!oldStatus.Value.IsActive && def.activeMote != null) {
            if (activeMote == null || activeMote.Destroyed)
                activeMote = MoteMaker.MakeAttachedOverlay(pawn, def.activeMote, Vector3.zero, GetMoteScale());
        } else if (!newStatus.Value.IsActive) {
            activeMote?.Destroy();
            activeMote = null;
        }

        if (activeMote != null) {
            activeMote.Scale = GetMoteScale();
        }

        OnStatusChanged(oldStatus.Value, newStatus.Value);

        nextStatus = null;
    }

    public new virtual bool GizmosVisible() => base.GizmosVisible();

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
                    Logger.Error("Could not instantiate or initialize an AbilityComp: " + ex);
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
        if (!pawn.Spawned) return;

        base.AbilityTick();

        activeMote?.Maintain();
        HandleAutoBurn();
    }

    protected virtual void HandleAutoBurn() {
        bool isDowned = pawn.Downed;
        bool isAsleep = pawn.IsAsleep();

        ApplyAutoTriggers(isDowned);

        if (!status.IsActive) {
            CommitStateSnapshot(isDowned, isAsleep, paused);
            return;
        }

        if (isAsleep) {
            if (!def.canUseWhileAsleep) {
                UpdateStatus(Active.Off);
            } else if (status.IsPoweredUp) {
                UpdateStatus(Active.On);
            }
        } else if (paused) {
            paused = false;
            if (lastWasPaused) {
                Gene.UpdateDrainSource((def, GetDesiredBurnRateForStatus(status)));
            }

            OnEnable();
            if (status.IsPoweredUp) OnPowerUp();
            CommitStateSnapshot(isDowned, isAsleep, paused: false);
            return;
        }

        if (isDowned) {
            if (status.IsPoweredUp && (!def.canUseWhileDowned || !willUseWhileDowned)) {
                UpdateStatus(Active.Off);
            } else if (status == Active.Off && willUseWhileDowned) {
                UpdateStatus(Active.On);
            }
        }

        CommitStateSnapshot(isDowned, isAsleep, paused);
    }

    private void ApplyAutoTriggers(bool isDowned) {
        if (willUseWhileDowned && isDowned && !pawn.Dead && status.IsActive) {
            if (Gene.CanLowerReserve(def.beuPerTick)) {
                UpdateStatus(Active.On);
            }
        }

        if (willUseWhileInjured && Gene.CanLowerReserve(def.beuPerTick)) {
            if (pawn.health.summaryHealth.SummaryHealthPercent < 1) UpdateStatus(Active.On);
            List<Verse.Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++) {
                if (hediffs[i].CanBeHealedByInvestiture()) {
                    UpdateStatus(Active.On);
                    break;
                }
            }
        }
    }

    private void CommitStateSnapshot(bool isDowned, bool isAsleep, bool paused) {
        lastWasDowned = isDowned;
        lastWasAsleep = isAsleep;
        lastWasPaused = paused;
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
        Gene.UpdateDrainSource((def, GetDesiredBurnRateForStatus(newStatus)));

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
        if (status.power > 1 && status.power > oldStatus.power) {
            OnPowerUp();
        }

        // When de-flaring to burning
        if (oldStatus.power > 1 && status.power < oldStatus.power) {
            OnPowerDown();
        }

        OnStatusChangedEvent?.Invoke(this, oldStatus, newStatus);
    }

    public override bool GizmoDisabled(out string reason) {
        if (!status.IsActive) return base.GizmoDisabled(out reason);

        reason = string.Empty;
        return false;
    }

    /// <summary>
    ///     We hide all gizmos for this ability because they are added by the gene.
    /// </summary>
    /// <returns></returns>
    public override IEnumerable<Command> GetGizmos() {
        yield break;
    }

    protected virtual void OnEnable() { }

    protected virtual void OnDisable() {
        nextStatus = null;
        OnPowerDown();
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

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Deep.Look(ref status, "status");

        switch (Scribe.mode) {
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

    public override void QueueCastingJob(LocalTargetInfo targetInfo, LocalTargetInfo destination) {
        QueueCastingJob(targetInfo, destination, 1);
    }

    public override void QueueCastingJob(GlobalTargetInfo targetInfo) {
        QueueCastingJob(targetInfo, 1);
    }

    public void QueueCastingJob(LocalTargetInfo targetInfo, LocalTargetInfo destination, int power) {
        localTarget = targetInfo;
        SetNextStatus((Status)power);

        base.QueueCastingJob(targetInfo, destination);
    }

    public void QueueCastingJob(GlobalTargetInfo targetInfo, int power) {
        globalTarget = targetInfo;
        SetNextStatus((Status)power);

        base.QueueCastingJob(targetInfo);
    }

    public override Job GetJob(LocalTargetInfo targetInfo, LocalTargetInfo destination) {
        if (nextStatus != null && def.toggleable) {
            UpdateStatus(nextStatus.Value);
        }

        Job? job = JobMaker.MakeJob(def.jobDef ?? RimWorld.JobDefOf.CastAbilityOnThing, targetInfo);
        job.source = this;
        job.ability = this;
        job.verbToUse = verb;
        job.playerForced = true;
        job.targetA = targetInfo;
        job.targetB = destination;
        job.count = nextStatus?.power ?? status.power;
        job.followRadius = def.verbProperties.range / 2f;

        return job;
    }

    protected override void PreActivate(LocalTargetInfo? target) {
        UpdateStatus();

        base.PreActivate(target);
    }

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo destination) {
        bool result = base.Activate(target, destination);
        nextStatus = null;

        if (!def.toggleable) {
            UpdateStatus(Active.Off);
        }

        return result;
    }
}
