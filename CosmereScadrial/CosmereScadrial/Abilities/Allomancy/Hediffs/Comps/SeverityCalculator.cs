using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy.Hediffs.Comps;

public class SeverityCalculatorProperties : HediffCompProperties {
    public float decayAmount = 0.05f;
    public int decayInterval = GenTicks.TicksPerRealSecond;
    public bool onPostAdd = false;
    public bool onStatusChange = true;
    public bool onTickInterval = false;
    public bool shouldDecay = false;
    public int tickInterval = GenTicks.TickRareInterval;

    public SeverityCalculatorProperties() {
        compClass = typeof(SeverityCalculator);
    }
}

public class SeverityCalculator : HediffComp {
    private float desiredSeverity = -1;

    private bool removeOnNextChange;
    private new SeverityCalculatorProperties props => (SeverityCalculatorProperties)base.props;

    private new AllomanticHediff parent => (AllomanticHediff)base.parent;
    public float severity => parent.sourceAbilities.Sum(x => x.GetStrength()) + parent.extraSeverity;

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

        if (!props.onStatusChange) return;
        parent.OnSourceAdded += OnSourceAdded;
        parent.OnSourceRemoved += OnSourceRemoved;
    }

    public override void CompPostPostAdd(DamageInfo? dinfo) {
        base.CompPostPostAdd(dinfo);
        if (props.onPostAdd) RecalculateSeverity();
    }


    private void OnSourceRemoved(AllomanticHediff hediff, AbstractAbility sourceAbility) {
        parent.OnSourceAdded -= OnSourceAdded;
        parent.OnSourceRemoved -= OnSourceRemoved;
    }

    private void OnSourceAdded(AllomanticHediff hediff, AbstractAbility sourceAbility) {
        if (desiredSeverity >= 0) desiredSeverity += sourceAbility.GetStrength();
        sourceAbility.OnStatusChanged += OnStatusChange;
        if (props.onStatusChange) RecalculateSeverity();
    }

    private void OnStatusChange(AbstractAbility sourceAbility, BurningStatus oldStatus, BurningStatus newStatus) {
        if (newStatus == BurningStatus.Off) {
            parent.RemoveSource(sourceAbility);
        }

        if (newStatus > oldStatus || props is { shouldDecay: false, onStatusChange: true }) {
            desiredSeverity = -1;
            RecalculateSeverity();
            return;
        }

        desiredSeverity = severity;
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        if (props.onTickInterval && Pawn.IsHashIntervalTick(props.tickInterval, delta)) RecalculateSeverity();

        if (!props.shouldDecay || desiredSeverity < 0) return;
        if (Mathf.Approximately(parent.Severity, desiredSeverity) || parent.Severity <= desiredSeverity) {
            desiredSeverity = -1;
            return;
        }

        if (!Pawn.IsHashIntervalTick(props.decayInterval, delta)) return;
        parent.Severity = Mathf.Max(desiredSeverity, parent.Severity - props.decayAmount);
    }

    public void RecalculateSeverity() {
        parent.Severity = severity;
    }
}