using System;
using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.Core.Investiture;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Allomancy.Hediff;

public class SurgeChargeHediff : AllomanticHediff {
    public Action? endCallback;

    public int endInTicks = -1;

    public SurgeChargeHediff() { }

    public SurgeChargeHediff(HediffDef d, Pawn p, IAbility<Allomancer, IHediff<Allomancer>> a) : base(d, p, a) { }

    public override void Tick() {
        base.Tick();
        if (endInTicks == -1) return;
        if (endInTicks == 0) {
            PostBurn();
            return;
        }

        endInTicks--;
    }

    public void Burn(Action? callback = null, int endInTicks = -1, Action? endCallback = null) {
        if (this.endInTicks > -1) return;
        if (pawn.genes == null) return;

        List<Allomancer> genes = pawn.genes.GetAllomanticGenes();
        for (int i = 0; i < genes.Count; i++) {
            if (!genes[i].Burning) continue;
            List<DrainSource> sources = genes[i].Sources;
            for (int j = 0; j < sources.Count; j++) {
                if (((AllomanticAbilityDef)sources[j].Def).metal == metal) continue;
                pawn.GetAllomanticAbility(sources[j].Def)?.UpdateStatus(BurningStatus.Duralumin);
            }
        }

        callback?.Invoke();

        this.endInTicks = endInTicks;
        this.endCallback = endCallback;
    }

    public void PostBurn() {
        if (pawn.genes == null) return;
        List<Allomancer> postGenes = pawn.genes.GetAllomanticGenes();
        for (int i = 0; i < postGenes.Count; i++) {
            if (postGenes[i].Burning) {
                postGenes[i].WipeReserve();
            }
        }

        FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 1.2f);

        IAbility<Allomancer, IHediff<Allomancer>>[] snapshot = [.. SourceAbilities];
        for (int i = 0; i < snapshot.Length; i++) {
            if (snapshot[i] is not AllomancyAbility sourceAbility || !sourceAbility.atLeastBurning) continue;
            if (sourceAbility.pawn.genes == null) continue;
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
