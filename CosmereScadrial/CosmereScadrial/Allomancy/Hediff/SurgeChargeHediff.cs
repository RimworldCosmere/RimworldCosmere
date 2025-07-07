using System;
using System.Linq;
using CosmereFramework.Extension;
using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Allomancy.Comp.Thing;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using RimWorld;
using Verse;

namespace CosmereScadrial.Allomancy.Hediff;

public class SurgeChargeHediff(HediffDef d, Pawn p, AbstractAbility a) : AllomanticHediff(d, p, a) {
    public Action? endCallback;

    public int endInTicks = -1;

    private MetalBurning burning => pawn.GetComp<MetalBurning>();

    public override void Tick() {
        base.Tick();

        switch (endInTicks) {
            case -1:
                return;
            case 0:
                PostBurn();
                break;
        }

        if (endInTicks > 0) endInTicks--;
    }

    public void Burn(Action? callback = null, int endInTicks = -1, Action? endCallback = null) {
        if (this.endInTicks > -1) return;

        foreach (((MetallicArtsMetalDef _, AllomanticAbilityDef abilityDef), float rate) in
                 burning.burnSourceMap.ToList()) {
            if (rate <= 0f) continue;

            RimWorld.Ability? ability = pawn.abilities.GetAbility(abilityDef);
            if (ability is not AbstractAbility allomanticAbility) continue;

            allomanticAbility.UpdateStatus(BurningStatus.Duralumin);
        }

        callback?.Invoke();

        this.endInTicks = endInTicks;
        this.endCallback = endCallback;
    }

    public void PostBurn() {
        foreach (MetallicArtsMetalDef m in burning.GetBurningMetals()) {
            pawn.genes.GetAllomanticGeneForMetal(m)?.WipeReserve();
        }

        FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 1.2f);

        foreach (AbstractAbility? sourceAbility in sourceAbilities.ToList()) {
            Allomancer? sourceGene = sourceAbility.pawn.genes.GetAllomanticGeneForMetal(sourceAbility.metal);
            if (
                sourceAbility.def.IsOneOf(
                    AbilityDefOf.Cosmere_Scadrial_Ability_Duralumin,
                    AbilityDefOf.Cosmere_Scadrial_Ability_Nicrosil
                )
            ) {
                sourceGene?.WipeReserve();
            }

            sourceAbility.UpdateStatus(BurningStatus.Off);
        }

        pawn.health?.RemoveHediff(this);
        endCallback?.Invoke();
        endCallback = null;
        endInTicks = -1;
    }
}