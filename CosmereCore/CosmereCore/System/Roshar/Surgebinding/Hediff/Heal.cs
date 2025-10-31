using Cosmere;
using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Roshar.Gene;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Hediff;

public class Heal : SurgebindingHediff {
    public Heal() { }

    public Heal(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability) : base(
        hediffDef,
        pawn,
        ability
    ) {
        this.ability = ability;
    }

    public override void PostTickInterval(int delta) {
        base.PostTickInterval(delta);

        if (!gene.CanLowerReserve(ability.def.beuPerTick)) ability.UpdateStatus(Active.Off);
        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond / 2, delta)) return;

        List<Verse.Hediff> hediffs = pawn.health.hediffSet.hediffs
            .Where(h => h.CanBeHealedByInvestiture())
            .ToList();

        CheckIfDone(hediffs);

        foreach (Verse.Hediff hediff in hediffs) {
            if (!hediff.TryHealWithInvestiture(ability.gene.currentIdealDisplay)) {
                continue;
            }

            if (hediff.ShouldRemove) {
                hediffs.Remove(hediff);
            }

            pawn.skills.Learn(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower, 5);
            CheckIfDone(hediffs);
            return;
        }
    }

    private void CheckIfDone(List<Verse.Hediff> hediffs) {
        if (hediffs.Count != 0) return;
        if (!Mathf.Approximately(pawn.health.summaryHealth.SummaryHealthPercent, 1)) return;

        ability.UpdateStatus(Active.Off);
    }
}