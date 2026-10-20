using System;
using Cosmere.Core.Ability;
using Cosmere.Core.Gene;
using Cosmere.Core.Hediff;
using Cosmere.Core.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Comp.Hediff;

public class SeverityCalculatorProperties : HediffCompProperties {
    public float decayAmount = 0.05f;
    public int decayInterval = GenTicks.TicksPerRealSecond;

    /// <summary>
    ///     Each further source counts for half of the one before it, instead of adding whole.
    /// </summary>
    /// <remarks>
    ///     For effects several Allomancers can lay on one pawn at once, where the honest sum climbs
    ///     without limit and a colony could stack the effect to absurdity by rostering enough
    ///     bodies. Off everywhere else, so nothing that already adds whole quietly changes.
    /// </remarks>
    public bool diminishingStack = false;
    public bool onPostAdd = false;
    public bool onStatusChange = true;
    public bool onTickInterval = false;
    public bool shouldDecay = false;
    public int tickInterval = GenTicks.TickRareInterval;
}

public class SeverityCalculator<TGene> : HediffComp
    where TGene : Invested {
    private float cachedSeverity = -1;
    private float desiredSeverity = -1;
    private bool severityDirty = true;

    private SeverityCalculatorProperties Props => (SeverityCalculatorProperties)props;

    private IHediff<TGene> Parent => (IHediff<TGene>)parent;

    public float Severity {
        get {
            if (!severityDirty) return cachedSeverity;

            float total = Parent.ExtraSeverity;
            if (Props.diminishingStack) {
                List<float> sources = [];
                foreach (IAbility<TGene, IHediff<TGene>> source in Parent.SourceAbilities) {
                    sources.Add(source.GetStrength());
                }

                total += DiminishingStack.Combine(sources);
            } else {
                foreach (IAbility<TGene, IHediff<TGene>> source in Parent.SourceAbilities) {
                    total += source.GetStrength();
                }
            }

            cachedSeverity = total;
            severityDirty = false;
            return cachedSeverity;
        }
    }

    public override string CompLabelInBracketsExtra =>
        TicksLeft >= 0 ? $"{Mathf.RoundToInt(TicksLeft).ToStringTicksToPeriod()} left" : string.Empty;

    private float TicksLeft => desiredSeverity < 0
        ? -1
        : Mathf.Abs(Parent.Severity - desiredSeverity) / Props.decayAmount * Props.decayInterval;

    private void MarkSeverityDirty() {
        severityDirty = true;
    }

    public override void CompPostMake() {
        base.CompPostMake();

        if (Parent == null) {
            throw new InvalidOperationException(
                $"SeverityCalculator<{typeof(TGene).Name}> can only be placed on a hediff that implements IHediff<{typeof(TGene).Name}>"
            );
        }

        if (!Props.onStatusChange) {
            return;
        }

        Parent.OnSourceAdded += OnSourceAdded;
        Parent.OnSourceRemoved += OnSourceRemoved;
    }

    public override void CompPostPostAdd(DamageInfo? dinfo) {
        base.CompPostPostAdd(dinfo);
        if (Props.onPostAdd) {
            RecalculateSeverity();
        }
    }

    private void OnSourceRemoved(
        IHediff<TGene> hediff,
        IAbility<TGene, IHediff<TGene>> sourceAbility
    ) {
        MarkSeverityDirty();
        sourceAbility.OnStatusChangedEvent -= OnStatusChange;
        if (Parent.SourceAbilities.Count == 0) {
            Parent.OnSourceAdded -= OnSourceAdded;
            Parent.OnSourceRemoved -= OnSourceRemoved;
        }
    }

    private void OnSourceAdded(
        IHediff<TGene> hediff,
        IAbility<TGene, IHediff<TGene>> sourceAbility
    ) {
        if (desiredSeverity >= 0) {
            desiredSeverity += sourceAbility.GetStrength();
        }

        MarkSeverityDirty();
        sourceAbility.OnStatusChangedEvent += OnStatusChange;
        if (Props.onStatusChange) {
            RecalculateSeverity();
        }
    }

    private void OnStatusChange(
        IAbility<TGene, IHediff<TGene>> sourceAbility,
        Status oldStatus,
        Status newStatus
    ) {
        if (newStatus.active == Active.Off) {
            Parent.RemoveSource(sourceAbility);
        }

        MarkSeverityDirty();
        if (newStatus.power > oldStatus.power || Props is { shouldDecay: false, onStatusChange: true }) {
            desiredSeverity = -1;
            RecalculateSeverity();
            return;
        }

        desiredSeverity = Severity;
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        if (Props.onTickInterval && Pawn.IsHashIntervalTick(Props.tickInterval, delta)) {
            RecalculateSeverity();
        }

        if (!Props.shouldDecay || desiredSeverity < 0) {
            return;
        }

        if (Mathf.Approximately(Parent.Severity, desiredSeverity) || Parent.Severity <= desiredSeverity) {
            desiredSeverity = -1;
            return;
        }

        if (!Pawn.IsHashIntervalTick(Props.decayInterval, delta)) {
            return;
        }

        Parent.Severity = Mathf.Max(desiredSeverity, Parent.Severity - Props.decayAmount);
    }

    public void RecalculateSeverity() {
        Parent.Severity = Severity;
    }
}
