using Cosmere.Core.Ability;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Hediff;
using Cosmere.Roshar.Gene;
using UnityEngine;
using Verse;
using Logger = Cosmere.Framework.Logger;

namespace Cosmere.Roshar.Surgebinding.Hediff;

public class BreathStormlight(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability)
    : SurgebindingHediff(hediffDef, pawn, ability) {
    private const float MaxDrawDistance = 5f;
    private const float BaseAbsorbAmount = 1f;

    private InvestitureHolder investiture => pawn.GetComp<InvestitureHolder>()!;

    public override void PostTickInterval(int delta) {
        base.PostTickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) return;
        if (investiture.isFull) return;

        float amountDrawn = pawn.apparel.WornApparel.Sum(TryAbsorbFromThing);
        amountDrawn += pawn.equipment.AllEquipmentListForReading.Sum(TryAbsorbFromThing);
        amountDrawn += pawn.inventory.innerContainer.Sum(TryAbsorbFromThing);
        amountDrawn += pawn.GetCellsAround(MaxDrawDistance)
            .Sum(cell => cell.GetThingList(pawn.Map).Sum(TryAbsorbFromThing));

        Logger.Verbose($"{pawn.NameFullColored} as absorbed {amountDrawn:F2} stormlight.");
    }

    private float TryAbsorbFromThing(Verse.Thing thing) {
        if (!thing.TryGetComp(out InvestitureHolder investitureHolder)) return 0f;
        if (!investitureHolder.sharingInvestiture) return 0f;
        if (Mathf.Approximately(investitureHolder.currentInvestiture, 0f)) return 0f;
        if (investiture.AbsorbInvestitureFrom(
                thing,
                BaseAbsorbAmount * (gene.currentIdeal + 1),
                out float amountDrawn
            )) {
            pawn.skills.Learn(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower, .1f);
        }

        return amountDrawn;
    }
}