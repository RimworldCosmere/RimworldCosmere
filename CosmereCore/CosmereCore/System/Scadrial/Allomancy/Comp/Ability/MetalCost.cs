using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Allomancy.Comp.Ability;

public class MetalCostProperties : CompProperties_AbilityEffect {
    public MetalCostProperties() {
        compClass = typeof(MetalCost);
    }
}

public class MetalCost : CompAbilityEffect {
    private new AllomancyAbility parent => (AllomancyAbility)base.parent;
    private MetallicArtsMetalDef metal => parent.def.metal.ToMetallicArts();
    private Allomancer gene => parent.Gene;

    private float currentCost => parent.GetDesiredBurnRateForStatus(parent.nextStatus);

    private bool hasEnoughMetal => gene.CanBurn(currentCost);

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        base.Apply(target, dest);
        gene.RemoveFromReserve(currentCost);
    }

    public override bool GizmoDisabled(out string? reason) {
        AcceptanceReport canUseMetal = parent.pawn.CanUseMetal(metal);
        if (!canUseMetal) {
            reason = canUseMetal.Reason;
            return true;
        }

        AcceptanceReport canBurn = gene.CanBurn(currentCost);
        if (!canBurn.Accepted) {
            reason = canBurn.Reason;
            return true;
        }

        reason = null;
        return false;
    }

    public override bool AICanTargetNow(LocalTargetInfo target) {
        return hasEnoughMetal;
    }
}