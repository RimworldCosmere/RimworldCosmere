using System;
using Cosmere.Core.Ability;
using Cosmere.Core.Investiture;
using Cosmere.Scadrial.Allomancy.Ability;
using Cosmere.Scadrial.Def;
using Cosmere.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.Scadrial.Allomancy.Hediff;

public class SurgeChargeHediff(HediffDef d, Pawn p, AbstractAbility<Allomancer> a) : AllomanticHediff(d, p, a) {
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

        foreach (DrainSource source in pawn.genes.GetAllomanticGenes()
                     .Where(g => g.Burning)
                     .SelectMany(g => g.Sources)
                     .Where(s => ((AllomanticAbilityDef)s.Def).metal != metal)
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

        foreach (AllomancyAbility? sourceAbility in sourceAbilities.Cast<AllomancyAbility>()
                     .Where(a => a.atLeastBurning)
                     .ToList()) {
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