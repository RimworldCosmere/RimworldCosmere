using Cosmere.Core.Ability;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Hediff;
using Cosmere.System.Roshar.Gene;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Hediff;

public class BreatheStormlight : SurgebindingHediff {
    private const float MaxDrawDistance = 5f;
    private const float BaseAbsorbAmount = 1f;

    public BreatheStormlight() { }

    public BreatheStormlight(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability) : base(hediffDef, pawn, ability) { }

    public override void PostTickInterval(int delta) {
        base.PostTickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) return;
        if (investiture == null || investiture.isFull) return;

        if (pawn.apparel?.WornApparel != null) {
            AbsorbFromAll(pawn.apparel.WornApparel);
        }

        if (pawn.equipment?.AllEquipmentListForReading != null) {
            AbsorbFromAll(pawn.equipment.AllEquipmentListForReading);
        }

        if (pawn.inventory?.innerContainer != null) {
            AbsorbFromAll(pawn.inventory.innerContainer);
        }

        if (pawn.Spawned && pawn.Map != null) {
            foreach (IntVec3 cell in pawn.GetCellsAround(MaxDrawDistance)) {
                AbsorbFromAll(cell.GetThingList(pawn.Map));
            }
        }
    }

    private void AbsorbFromAll(IEnumerable<Verse.Thing> things) {
        foreach (Verse.Thing thing in things) TryAbsorbFromThing(thing);
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
