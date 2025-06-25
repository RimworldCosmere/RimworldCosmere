using System;
using System.Linq;
using CosmereScadrial.Comp.Thing;
using CosmereScadrial.Def;
using RimWorld;
using Verse;

namespace CosmereScadrial.Ability.Allomancy.Hediff;

public class SurgeChargeHediff(HediffDef hediffDef, Pawn pawn, AbstractAbility ability)
    : AllomanticHediff(hediffDef, pawn, ability) {
    public Action? endCallback;

    public int endInTicks = -1;

    private MetalBurning burning => pawn.GetComp<MetalBurning>();
    private MetalReserves reserves => pawn.GetComp<MetalReserves>();

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

    public void Burn(Action callback = null, int endInTicks = -1, Action endCallback = null) {
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
        burning.GetBurningMetals().ForEach(reserves.RemoveReserve);
        FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 1.2f);

        foreach (AbstractAbility? sourceAbility in sourceAbilities.ToList()) {
            MetalReserves? res = sourceAbility.pawn.GetComp<MetalReserves>();
            if (sourceAbility.def.Equals(AbilityDefOf.Cosmere_Scadrial_Ability_DuraluminSurge)) {
                res.RemoveReserve(MetallicArtsMetalDefOf.Duralumin);
            }

            if (sourceAbility.def.Equals(AbilityDefOf.Cosmere_Scadrial_Ability_NicrosilSurge)) {
                res.RemoveReserve(MetallicArtsMetalDefOf.Nicrosil);
            }

            sourceAbility.UpdateStatus(BurningStatus.Off);
        }

        pawn.health?.RemoveHediff(this);
        endCallback?.Invoke();
        endCallback = null;
        endInTicks = -1;
    }
}