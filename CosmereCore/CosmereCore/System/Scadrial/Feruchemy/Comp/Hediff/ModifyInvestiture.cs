using Cosmere.Core;
using Cosmere.Core.Need;
using Cosmere.System.Scadrial.Gene;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Feruchemy.Comp.Hediff;

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
    private Feruchemist? nicrosil => parent.pawn.genes?.GetFeruchemicGeneForMetal(MetalDefOf.Nicrosil);

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

    private bool RemoveSelfIfNoInvestiture() {
        if (investiture != null) return false;
        Logger.Error("CS_Error_CannotModifyInvestiture".Translate());
        parent.pawn.health.RemoveHediff(parent);
        return true;
    }

    public override void CompPostMake() {
        base.CompPostMake();
        if (RemoveSelfIfNoInvestiture()) return;

        if (shouldResetNicrosil) nicrosil?.Reset();
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        Investiture? inv = investiture;
        if (inv == null) {
            Logger.Error("CS_Error_CannotModifyInvestiture".Translate());
            parent.pawn.health.RemoveHediff(parent);
            return;
        }

        if (shouldResetNicrosil) {
            nicrosil?.Reset();
            return;
        }

        base.CompPostTickInterval(ref severityAdjustment, delta);

        float changePerTick = props.changePerRareTick / GenTicks.TickRareInterval;
        if (props.multiplyBySeverity) {
            changePerTick *= parent.Severity;
        }

        inv.CurLevel = Mathf.Clamp(inv.CurLevel - changePerTick, 0, inv.MaxLevel);
    }
}