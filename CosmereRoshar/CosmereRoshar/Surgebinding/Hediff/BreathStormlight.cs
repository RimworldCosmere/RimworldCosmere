using Cosmere.Core.Ability;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Hediff;
using Cosmere.Roshar.Gene;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Framework.Logger;

namespace Cosmere.Roshar.Surgebinding.Hediff;

public class BreathStormlight(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability)
    : SurgebindingHediff(hediffDef, pawn, ability) {
    private const float maxDrawDistance = 3f;
    private const float BaseAbsorbAmount = 25f;

    private InvestitureHolder? investiture => pawn.GetComp<InvestitureHolder>();

    public override void PostTickInterval(int delta) {
        base.PostTickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) return;
        if (investiture == null || investiture.isFull) return;

        float amountDrawn = 0f;
        foreach (Apparel apparel in pawn.apparel.WornApparel) {
            amountDrawn += TryAbsorbFromThing(apparel);
        }

        foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading) {
            amountDrawn += TryAbsorbFromThing(equipment);
        }

        foreach (Verse.Thing thing in pawn.inventory.innerContainer) {
            amountDrawn += TryAbsorbFromThing(thing);
        }

        foreach (IntVec3 cell in pawn.GetCellsAround(maxDrawDistance)) {
            foreach (Verse.Thing thing in cell.GetThingList(pawn.Map).Where(t => t.HasComp<InvestitureHolder>())) {
                amountDrawn += TryAbsorbFromThing(thing);
            }
        }

        Logger.Verbose($"{pawn.NameFullColored} as absorbed {amountDrawn:F2} stormlight.");
    }

    private float TryAbsorbFromThing(Verse.Thing thing) {
        if (!thing.TryGetComp(out InvestitureHolder investitureHolder)) return 0f;
        if (Mathf.Approximately(investitureHolder.currentInvestiture, 0f)) return 0f;
        if (investitureHolder.ExudeInvestitureInto(
                pawn,
                BaseAbsorbAmount * (gene.currentIdeal + 1),
                out float amountDrawn
            )) {
            pawn.skills.Learn(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower, .1f);
        }

        return amountDrawn;
    }
}