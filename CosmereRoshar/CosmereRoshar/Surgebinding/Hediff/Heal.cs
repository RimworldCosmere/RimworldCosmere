using Cosmere.Core.Ability;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Hediff;
using Cosmere.Roshar.Gene;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Surgebinding.Hediff;

public class Heal(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability)
    : SurgebindingHediff(hediffDef, pawn, ability) {
    private const float MaxDrawDistance = 5f;
    private const float BaseAbsorbAmount = 1f;

    private InvestitureHolder investiture => pawn.GetComp<InvestitureHolder>()!;

    public override void PostTickInterval(int delta) {
        base.PostTickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) return;

        List<Hediff_Injury> injuries = pawn.health.hediffSet.hediffs
            .OfType<Hediff_Injury>()
            .Where(i => i.CanBeHealedWithInvestiture())
            .ToList();

        CheckIfDone(injuries);

        foreach (Hediff_Injury injury in injuries) {
            injury.Heal(Mathf.Lerp(0, 3, gene.currentIdeal + 1));
            if (injury.ShouldRemove) {
                injuries.Remove(injury);
            }

            CheckIfDone(injuries);
            return;
        }
    }

    private void CheckIfDone(List<Hediff_Injury> injuries) {
        if (injuries.Count != 0) return;
        if (!Mathf.Approximately(pawn.health.summaryHealth.SummaryHealthPercent, 1)) return;

        ability.UpdateStatus(Active.Off);
    }
}