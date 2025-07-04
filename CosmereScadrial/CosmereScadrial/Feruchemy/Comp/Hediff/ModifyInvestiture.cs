using System;
using CosmereCore.Need;
using CosmereResources;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Feruchemy.Comp.Hediff;

public class ModifyInvestitureProperties : HediffCompProperties {
    public float changePerRareTick = 0;
    public bool multiplyBySeverity = true;

    public ModifyInvestitureProperties() {
        compClass = typeof(ModifyInvestiture);
    }
}

public class ModifyInvestiture : HediffComp {
    public new ModifyInvestitureProperties props => (ModifyInvestitureProperties)base.props;
    private bool isNicrosil => parent.def.defName.EndsWith("Nicrosil");
    private Investiture? investiture => parent.pawn?.needs?.TryGetNeed<Investiture>();
    private Feruchemist? nicrosil => parent.pawn.genes.GetFeruchemicGeneForMetal(MetalDefOf.Nicrosil);

    private bool isTapping => props.changePerRareTick < 0;
    private bool isStoring => props.changePerRareTick > 0;

    private bool shouldResetNicrosil {
        get {
            if (investiture == null) return false;
            if (!isNicrosil || nicrosil == null) return false;

            if (isTapping) {
                if (!nicrosil.canTap || investiture.CurLevel >= investiture.MaxLevel) return true;
            }

            if (isStoring) {
                if (!nicrosil.canStore || investiture.CurLevel <= 0) return true;
            }

            return false;
        }
    }

    public override void CompPostMake() {
        base.CompPostMake();
        if (investiture == null)
            throw new Exception("Modifying investiture on on an hediff tree that doesnt have investiture");

        if (shouldResetNicrosil) nicrosil?.Reset();
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        if (investiture == null)
            throw new Exception("Modifying investiture on on an hediff tree that doesnt have investiture");
        if (shouldResetNicrosil) {
            nicrosil?.Reset();
            return;
        }

        base.CompPostTickInterval(ref severityAdjustment, delta);

        float changePerTick = props.changePerRareTick / GenTicks.TickRareInterval;
        if (props.multiplyBySeverity) {
            changePerTick *= parent.Severity;
        }

        investiture.CurLevel = Mathf.Clamp(investiture.CurLevel - changePerTick, 0, investiture.MaxLevel);
    }
}