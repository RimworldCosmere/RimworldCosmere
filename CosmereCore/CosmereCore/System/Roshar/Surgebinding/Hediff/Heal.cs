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

        if (!Gene.CanLowerReserve(ability.def.beuPerTick)) ability.UpdateStatus(Active.Off);
        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond / 2, delta)) return;

        List<Verse.Hediff> hediffs = pawn.health.hediffSet.hediffs
            .Where(h => h.CanBeHealedByInvestiture())
            .ToList();

        DeactivateIfFullyHealed(hediffs);

        foreach (Verse.Hediff hediff in hediffs) {
            if (!hediff.TryHealWithInvestiture(ability.Gene.CurrentIdealDisplay)) {
                continue;
            }

            if (hediff.ShouldRemove) {
                hediffs.Remove(hediff);
            }

            pawn.skills.Learn(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower, 5);
            DeactivateIfFullyHealed(hediffs);
            return;
        }
    }

    private void DeactivateIfFullyHealed(List<Verse.Hediff> hediffs) {
        if (hediffs.Count != 0) return;
        if (!Mathf.Approximately(pawn.health.summaryHealth.SummaryHealthPercent, 1)) return;

        ability.UpdateStatus(Active.Off);
    }
}