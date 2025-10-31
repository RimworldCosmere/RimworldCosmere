using Cosmere;
using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Gene;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

public class Compound : AllomanticHediff {
    private readonly Allomancer? allomancer;
    private readonly Feruchemist? feruchemist;

    public Compound() { }

    public Compound(HediffDef hediffDef, Pawn pawn, IAbility<Allomancer, IHediff<Allomancer>> ability) : base(
        hediffDef,
        pawn,
        ability
    ) {
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

    private void End() {
        ability.UpdateStatus(BurningStatus.Off);
    }
}