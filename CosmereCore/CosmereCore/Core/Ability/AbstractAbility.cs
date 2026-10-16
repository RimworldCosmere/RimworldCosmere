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

    public Def_AbilityDef Def { get; }

    public void UpdateStatus(Status? newStatus = null);

    public float GetStrength(Status? desiredStatus = null);

    public event Action<IAbility<TGene, THediff>, Status, Status>? OnStatusChangedEvent;
}

public abstract class AbstractAbility(Pawn pawn, AbilityDef def)
    : AbstractAbility<Invested, AbstractHediff<Invested>>(pawn, def);

public abstract class AbstractAbility<TGene>(Pawn pawn, AbilityDef def)
    : AbstractAbility<TGene, AbstractHediff<TGene>>(pawn, def)
    where TGene : Invested;

public abstract class AbstractAbility<TGene, THediff> : RimWorld.Ability, IAbility<TGene, THediff>, IToggleableAbility
    where TGene : Invested
    where THediff : IHediff<TGene> {
    protected Mote? activeMote;
    protected TGene? cachedGene;

    public GlobalTargetInfo? globalTarget;
    protected Job? job;
    private bool lastWasPaused;
    public LocalTargetInfo? localTarget;
    public bool paused;
    public Status status = Active.Off;
    public bool willUseWhileDowned;
    public bool willUseWhileInjured;

    protected AbstractAbility(Pawn pawn) : base(pawn) { }

    protected AbstractAbility(Pawn pawn, AbilityDef def) : base(pawn, def) {
        Initialize();
    }

    public Status? NextStatus { get; protected set; }

    public override AcceptanceReport CanCast => Gene.CanLowerReserve(Def.beuPerTick);

    public bool IsToggleable => Def.toggleable;

    public bool IsActive => status.IsActive;

    public void TurnOff() {
        UpdateStatus(Active.Off);
    }

    public override string Tooltip {
        get {
            List<string> tooltipByLine = [.. base.Tooltip.Split('\n')];

            if (Def.beuPerTick > 0) {
                if (Def.toggleable) {
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

            return string.Join("\n", tooltipByLine);
        }
    }

    public Def_AbilityDef Def {
        get => (Def_AbilityDef)def;
        set => def = value;
    }

    public abstract TGene Gene { get; }

    public event Action<IAbility<TGene, THediff>, Status, Status>? OnStatusChangedEvent;

    public virtual float GetStrength(Status? desiredStatus = null) {
        float baseStrength = (desiredStatus ?? status).power * Def.hediffSeverityFactor;

        if (pawn.IsAsleep()) {
            return baseStrength * Def.asleepStrengthFactor;
        }

        if (pawn.Downed) {
            return baseStrength * Def.downedStrengthFactor;
        }

        return baseStrength;
    }

    public void UpdateStatus(Status? newStatus = null) {
        if (newStatus == null) {
            if (NextStatus == null) {
                Log.Error("Call to UpdateStatus with no status, and no next status");
                return;
            }

            newStatus = NextStatus;
        }

        if (status == newStatus) {
            return;
        }

        Status? oldStatus = status;
        status = newStatus.Value;

        if (!oldStatus.Value.IsActive && Def.activeMote != null) {
            if (activeMote == null || activeMote.Destroyed)
                activeMote = MoteMaker.MakeAttachedOverlay(pawn, Def.activeMote, Vector3.zero, GetMoteScale());
        } else if (!newStatus.Value.IsActive) {
            activeMote?.Destroy();
            activeMote = null;
        }

        if (activeMote != null) {
            activeMote.Scale = GetMoteScale();
        }

        OnStatusChanged(oldStatus.Value, newStatus.Value);

        NextStatus = null;
    }

    public new virtual bool GizmosVisible() {
        return base.GizmosVisible();
    }

    // not virtual: the ctor calls this, so an override would run against a half-built subclass.
    public new void Initialize() {
        if (Def.comps.Any<AbilityCompProperties>()) {
            comps = [];
            for (int index = 0; index < Def.comps.Count; ++index) {
                AbilityComp? abilityComp = null;
                try {
                    abilityComp = (AbilityComp)Activator.CreateInstance(Def.comps[index].compClass)!;
                    abilityComp.parent = this;
                    comps.Add(abilityComp);
                    abilityComp.Initialize(Def.comps[index]);
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

        willUseWhileDowned = Def is { canUseWhileDowned: true, autoUseWhileDowned: true };
        willUseWhileInjured = Def is { autoUseWhileInjured: true };

        if (Def.charges <= 0) {
            return;
        }

        maxCharges = Def.charges;
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
            lastWasPaused = paused;
            return;
        }

        if (isAsleep) {
            ApplyAsleepTransitions();
        } else if (paused) {
            paused = false;
            if (lastWasPaused) {
                Gene.UpdateDrainSource((Def, GetDesiredBurnRateForStatus(status)));
            }

            OnEnable();
            if (status.IsPoweredUp) OnPowerUp();
            lastWasPaused = false;
            return;
        }

        if (isDowned) {
            ApplyDownedTransitions();
        }

        lastWasPaused = paused;
    }

    private void ApplyAsleepTransitions() {
        if (!Def.canUseWhileAsleep) {
            UpdateStatus(Active.Off);
        } else if (status.IsPoweredUp) {
            UpdateStatus(Active.On);
        }
    }

    private void ApplyDownedTransitions() {
        if (status.IsPoweredUp && (!Def.canUseWhileDowned || !willUseWhileDowned)) {
            UpdateStatus(Active.Off);
        } else if (status == Active.Off && willUseWhileDowned) {
            UpdateStatus(Active.On);
        }
    }

    private void ApplyAutoTriggers(bool isDowned) {
        if (willUseWhileDowned && isDowned && !pawn.Dead && status.IsActive &&
            Gene.CanLowerReserve(Def.beuPerTick)) {
            UpdateStatus(Active.On);
        }

        if (willUseWhileInjured && Gene.CanLowerReserve(Def.beuPerTick)) {
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

    protected virtual float GetMoteScale() {
        return status.power;
    }

    public virtual float GetDesiredBurnRateForStatus() {
        return GetDesiredBurnRateForStatus(status);
    }

    public virtual float GetDesiredBurnRateForStatus(Status? desiredStatus) {
        return (desiredStatus ?? status).power * Def.beuPerTick;
    }

    public void SetNextStatus(Status desiredStatus, bool overrideNextStatus = false) {
        if (NextStatus != null && !overrideNextStatus) return;

        desiredStatus.power = Math.Clamp(desiredStatus.power, 0, Def.maxPower);

        NextStatus = desiredStatus;
    }

    protected virtual void OnStatusChanged(Status oldStatus, Status newStatus) {
        Gene.UpdateDrainSource((Def, GetDesiredBurnRateForStatus(newStatus)));

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
        NextStatus = null;
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

        // A Thing target only becomes real in ResolvingCrossRefs, so run every mode like Job does.
        LocalTargetInfo target = localTarget ?? LocalTargetInfo.Invalid;
        Scribe_TargetInfo.Look(ref target, "target");
        if (Scribe.mode != LoadSaveMode.Saving) localTarget = target.IsValid ? target : null;
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
        if (NextStatus != null && Def.toggleable) {
            UpdateStatus(NextStatus.Value);
        }

        Job? castJob = JobMaker.MakeJob(Def.jobDef ?? RimWorld.JobDefOf.CastAbilityOnThing, targetInfo);
        castJob.source = this;
        castJob.ability = this;
        castJob.verbToUse = verb;
        castJob.playerForced = true;
        castJob.targetA = targetInfo;
        castJob.targetB = destination;
        castJob.count = NextStatus?.power ?? status.power;
        castJob.followRadius = Def.verbProperties.range / 2f;

        return castJob;
    }

    protected override void PreActivate(LocalTargetInfo? target) {
        UpdateStatus();

        base.PreActivate(target);
    }

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo destination) {
        bool result = base.Activate(target, destination);
        NextStatus = null;

        if (!Def.toggleable) {
            UpdateStatus(Active.Off);
        }

        return result;
    }
}
