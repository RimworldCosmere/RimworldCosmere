using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Gene;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

public class Compound : AllomanticHediff {
    public Compound() { }

    public Compound(HediffDef hediffDef, Pawn pawn, IAbility<Allomancer, IHediff<Allomancer>> ability) : base(
        hediffDef,
        pawn,
        ability
    ) { }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        Allomancer? allomancer = pawn.genes?.GetAllomanticGeneForMetal(metal);
        Feruchemist? feruchemist = pawn.genes?.GetFeruchemicGeneForMetal(metal);

        if (allomancer == null || feruchemist == null || pawn.DeadOrDowned) {
            End();
            return;
        }

        if (!pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) return;

        float strength = ability.GetStrength(BurningStatus.Burning);
        float metalToBurn = ability.def.beuPerTick * GenTicks.TickRareInterval * strength;
        if (!TickLogic(feruchemist, delta, metalToBurn)) {
            End();
        }
    }

    protected virtual bool TickLogic(Feruchemist feruchemist, int delta, float metalToBurn) {
        return feruchemist.AddToStore(metalToBurn * 10f);
    }

    private void End() {
        ability.UpdateStatus(BurningStatus.Off);
    }
}
