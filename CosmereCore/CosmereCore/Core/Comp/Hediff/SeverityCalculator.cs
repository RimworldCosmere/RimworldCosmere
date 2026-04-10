using System;
using Cosmere.Core.Ability;
using Cosmere.Core.Gene;
using Cosmere.Core.Hediff;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Comp.Hediff;

public class SeverityCalculatorProperties : HediffCompProperties {
    public float decayAmount = 0.05f;
    public int decayInterval = GenTicks.TicksPerRealSecond;
    public bool onPostAdd = false;
    public bool onStatusChange = true;
    public bool onTickInterval = false;
    public bool shouldDecay = false;
    public int tickInterval = GenTicks.TickRareInterval;
}

public class SeverityCalculator<TGene> : HediffComp where TGene : Invested {
    private float desiredSeverity = -1;
    private float cachedSeverity = -1;
    private bool severityDirty = true;

    private new SeverityCalculatorProperties props => (SeverityCalculatorProperties)base.props;

    private new IHediff<TGene> parent => (IHediff<TGene>)base.parent;

    public float severity {
        get {
            if (!severityDirty) return cachedSeverity;
            float total = parent.extraSeverity;
            foreach (IAbility<TGene, IHediff<TGene>> source in parent.sourceAbilities) {
                total += source.GetStrength();
            }

            cachedSeverity = total;
            severityDirty = false;
            return cachedSeverity;
        }
    }

    private void MarkSeverityDirty() {
        severityDirty = true;
    }

    public override string CompLabelInBracketsExtra =>
        ticksLeft >= 0 ? $"{Mathf.RoundToInt(ticksLeft).ToStringTicksToPeriod()} left" : "";

    private float ticksLeft => desiredSeverity < 0
        ? -1
        : Mathf.Abs(parent.Severity - desiredSeverity) / props.decayAmount * props.decayInterval;

    public override void CompPostMake() {
        base.CompPostMake();

        if (parent == null) {
            throw new Exception("SeverityCalculator can only be placed on an AllomanticHediff");
        }

        if (!props.onStatusChange) {
            return;
        }

        parent.OnSourceAdded += OnSourceAdded;
        parent.OnSourceRemoved += OnSourceRemoved;
    }

    public override void CompPostPostAdd(DamageInfo? dinfo) {
        base.CompPostPostAdd(dinfo);
        if (props.onPostAdd) {
            RecalculateSeverity();
        }
    }


    private void OnSourceRemoved(
        IHediff<TGene> hediff,
        IAbility<TGene, IHediff<TGene>> sourceAbility
    ) {
        MarkSeverityDirty();
        parent.OnSourceAdded -= OnSourceAdded;
        parent.OnSourceRemoved -= OnSourceRemoved;
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
        if (props.onStatusChange) {
            RecalculateSeverity();
        }
    }

    private void OnStatusChange(
        IAbility<TGene, IHediff<TGene>> sourceAbility,
        Status oldStatus,
        Status newStatus
    ) {
        if (newStatus.active == Active.Off) {
            parent.RemoveSource(sourceAbility);
        }

        MarkSeverityDirty();
        if (newStatus > oldStatus || props is { shouldDecay: false, onStatusChange: true }) {
            desiredSeverity = -1;
            RecalculateSeverity();
            return;
        }

        desiredSeverity = severity;
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        if (props.onTickInterval && Pawn.IsHashIntervalTick(props.tickInterval, delta)) {
            RecalculateSeverity();
        }

        if (!props.shouldDecay || desiredSeverity < 0) {
            return;
        }

        if (Mathf.Approximately(parent.Severity, desiredSeverity) || parent.Severity <= desiredSeverity) {
            desiredSeverity = -1;
            return;
        }

        if (!Pawn.IsHashIntervalTick(props.decayInterval, delta)) {
            return;
        }

        parent.Severity = Mathf.Max(desiredSeverity, parent.Severity - props.decayAmount);
    }

    public void RecalculateSeverity() {
        parent.Severity = severity;
    }
}