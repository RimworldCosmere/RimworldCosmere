using System;
using System.Linq;
using CosmereFramework.Extension;
using CosmereScadrial.Allomancy.Ability;
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

        foreach (AllomanticBurnSource source in pawn.genes.GetAllomanticGenes()
                     .Where(g => g.Burning)
                     .SelectMany(g => g.Sources)
                     .Where(s => s.Def.metal != metal)
                     .ToList()) {
            pawn.GetAllomanticAbility(source.Def)?.UpdateStatus(BurningStatus.Duralumin);
        }

        callback?.Invoke();

        this.endInTicks = endInTicks;
        this.endCallback = endCallback;
    }

    public void PostBurn() {
        pawn.genes.GetAllomanticGenes().Where(g => g.Burning).ToList().ForEach(g => g.WipeReserve());

        FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 1.2f);

        foreach (AbstractAbility? sourceAbility in sourceAbilities.Where(a => a.atLeastBurning).ToList()) {
            Allomancer? sourceGene = sourceAbility.pawn.genes.GetAllomanticGeneForMetal(sourceAbility.metal);
            if (sourceGene == null || !sourceGene.Burning) continue;
            if (
                sourceAbility.def.IsOneOf(
                    AbilityDefOf.Cosmere_Scadrial_Ability_Duralumin,
                    AbilityDefOf.Cosmere_Scadrial_Ability_Nicrosil
                )
            ) {
                sourceGene.WipeReserve();
            }

            sourceAbility.UpdateStatus(BurningStatus.Off);
        }

        pawn.health?.RemoveHediff(this);
        endCallback?.Invoke();
        endCallback = null;
        endInTicks = -1;
    }
}