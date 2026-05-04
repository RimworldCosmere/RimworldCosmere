using Cosmere.Core.Ability;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Hediff;
using Cosmere.System.Roshar.Gene;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Surgebinding.Hediff;

public class BreatheStormlight : SurgebindingHediff {
    private const float MaxDrawDistance = 5f;
    private const float BaseAbsorbAmount = 1f;
    public BreatheStormlight() { }

    public BreatheStormlight(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability) :
        base(hediffDef, pawn, ability) { }

    public override void PostTickInterval(int delta) {
        base.PostTickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) return;
        if (investiture == null || investiture.isFull) return;

        float amountDrawn = 0f;
        if (pawn.apparel?.WornApparel != null) {
            amountDrawn += pawn.apparel.WornApparel.Sum(TryAbsorbFromThing);
        }

        if (pawn.equipment?.AllEquipmentListForReading != null) {
            amountDrawn += pawn.equipment.AllEquipmentListForReading.Sum(TryAbsorbFromThing);
        }

        if (pawn.inventory?.innerContainer != null) {
            amountDrawn += pawn.inventory.innerContainer.Sum(TryAbsorbFromThing);
        }

        if (pawn.Spawned && pawn.Map != null) {
            amountDrawn += pawn.GetCellsAround(MaxDrawDistance)
                .Sum(cell => cell.GetThingList(pawn.Map).Sum(TryAbsorbFromThing));
        }

    }

    private float TryAbsorbFromThing(Verse.Thing thing) {
        if (investiture == null) return 0f;
        if (!thing.TryGetComp(out InvestitureHolder investitureHolder)) return 0f;
        if (!investitureHolder.sharingInvestiture) return 0f;
        if (Mathf.Approximately(investitureHolder.currentInvestiture, 0f)) return 0f;
        if (investiture.AbsorbInvestitureFrom(
                thing,
                BaseAbsorbAmount * (Gene.CurrentIdeal + 1),
                out float amountDrawn
            )) {
            pawn.skills.Learn(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower, .1f);
        }

        return amountDrawn;
    }
}