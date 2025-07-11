using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Allomancy.Hediff;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using Verse;

namespace CosmereScadrial.Feruchemy.Hediff;

public class Compound : AllomanticHediff {
    protected readonly AbstractAbility ability;
    protected readonly Allomancer? allomancer;
    protected readonly Feruchemist? feruchemist;

    public Compound(HediffDef hediffDef, Pawn pawn, AbstractAbility ability) : base(hediffDef, pawn, ability) {
        this.ability = ability;
        allomancer = pawn.genes.GetAllomanticGeneForMetal(metal);
        feruchemist = pawn.genes.GetFeruchemicGeneForMetal(metal);
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (allomancer == null || feruchemist == null || pawn.DeadOrDowned) {
            End();
            return;
        }

        if (!pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) return;

        // Age the pawn biologically (store youth)
        float strength = ability.GetStrength(BurningStatus.Burning);
        float metalToBurn = ability.def.beuPerTick * GenTicks.TickRareInterval * strength;
        if (!TickLogic(delta, metalToBurn)) {
            End();
        }
    }

    protected virtual bool TickLogic(int delta, float metalToBurn) {
        // Add to Feruchemy reserve
        return feruchemist!.AddToStore(metalToBurn * 10f);
    }

    protected void End() {
        ability.UpdateStatus(BurningStatus.Off);
    }
}