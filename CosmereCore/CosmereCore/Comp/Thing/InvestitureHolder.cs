using UnityEngine;
using Verse;

namespace Cosmere.Core.Comp.Thing;

public class InvestitureHolderProperties : CompProperties {
    public float drainRate = 0f;
    public float maxInvestiture = Need.Investiture.MaxInvestiture;

    public InvestitureHolderProperties() {
        compClass = typeof(InvestitureHolder);
    }
}

public class InvestitureHolder : ThingComp {
    public float currentInvestiture;
    public float maxInvestiture;

    private new InvestitureHolderProperties props => (InvestitureHolderProperties)base.props;

    public override void PostPostMake() {
        base.PostPostMake();
        maxInvestiture = props.maxInvestiture;
    }

    public override string CompInspectStringExtra() {
        return "CC_Stored_Investiture".Translate() + $": {currentInvestiture:F0} / {maxInvestiture:F0}";
    }

    public override void CompTickInterval(int delta) {
        base.CompTickInterval(delta);

        if (Mathf.Approximately(props.drainRate, 0)) return;
        if (!GenTicks.IsTickIntervalDelta(GenTicks.TicksPerRealSecond, delta)) return;

        currentInvestiture -= props.drainRate;
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Values.Look(ref currentInvestiture, "currentInvestiture", 0f, true);
        Scribe_Values.Look(ref maxInvestiture, "maxInvestiture", 0f, true);
    }
}