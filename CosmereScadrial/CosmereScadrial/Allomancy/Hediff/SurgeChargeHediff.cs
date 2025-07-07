using System;
using System.Linq;
using CosmereFramework.Extension;
using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using RimWorld;
using Verse;

namespace CosmereScadrial.Allomancy.Hediff;

public class SurgeChargeHediff(HediffDef d, Pawn p, AbstractAbility a) : AllomanticHediff(d, p, a) {
    public Action? endCallback;

    public int endInTicks = -1;

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

        foreach (Allomancer gene in pawn.genes.GetAllomanticGenes()) {
            float rate = gene.BurnRate;
            if (rate <= 0f) continue;

            foreach ((AllomanticAbilityDef def, float amount) source in gene.Sources) {
                pawn.GetAllomanticAbility(source.def).UpdateStatus(BurningStatus.Duralumin);
            }
        }

        callback?.Invoke();

        this.endInTicks = endInTicks;
        this.endCallback = endCallback;
    }

    public void PostBurn() {
        pawn.genes.GetAllomanticGenes().ForEach(g => g.WipeReserve());

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