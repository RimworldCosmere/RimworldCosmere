using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Allomancy.Comp.Thing;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using RimWorld;
using Verse;

namespace CosmereScadrial.Allomancy.Comp.Ability;

public class MetalCostProperties : CompProperties_AbilityEffect {
    public MetalCostProperties() {
        compClass = typeof(MetalCost);
    }
}

public class MetalCost : CompAbilityEffect {
    private new AbstractAbility parent => (AbstractAbility)base.parent;
    private MetallicArtsMetalDef metal => parent.def.metal.ToMetallicArts();
    private Allomancer gene => parent.pawn.genes.GetAllomanticGeneForMetal(metal)!;
    private MetalBurning burning => parent.pawn.GetComp<MetalBurning>();

    private float currentCost => parent.GetDesiredBurnRateForStatus(parent.nextStatus);

    private bool hasEnoughMetal => burning.CanBurn(metal, currentCost);

    /*public override string ExtraTooltipPart() {
        return "CS_MetalCost".Translate() + $": {currentCost:0.000}";
    }*/

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

        AcceptanceReport canBurn = burning.CanBurn(metal, currentCost);
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