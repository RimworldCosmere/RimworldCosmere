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
    private AllomancyAbility Parent => (AllomancyAbility)parent;

    private MetallicArtsMetalDef Metal => Parent.Def.metal.ToMetallicArts();

    private Allomancer Gene => Parent.Gene;

    private float CurrentCost => Parent.GetDesiredBurnRateForStatus(Parent.NextStatus);

    private bool HasEnoughMetal => Gene.CanBurn(CurrentCost);

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        base.Apply(target, dest);
        Gene.RemoveFromReserve(CurrentCost);
    }

    public override bool GizmoDisabled(out string? reason) {
        AcceptanceReport canUseMetal = Parent.pawn.CanUseMetal(Metal);
        if (!canUseMetal) {
            reason = canUseMetal.Reason;
            return true;
        }

        AcceptanceReport canBurn = Gene.CanBurn(CurrentCost);
        if (!canBurn.Accepted) {
            reason = canBurn.Reason;
            return true;
        }

        reason = null;
        return false;
    }

    public override bool AICanTargetNow(LocalTargetInfo target) {
        return HasEnoughMetal;
    }
}
