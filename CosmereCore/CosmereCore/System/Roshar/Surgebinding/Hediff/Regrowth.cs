using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Hediff;

public class Regrowth : SurgebindingHediff {
    public Regrowth() { }

    public Regrowth(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability) : base(
        hediffDef,
        pawn,
        ability
    ) {
        this.ability = ability;
    }

    public override void PostTickInterval(int delta) {
        base.PostTickInterval(delta);

        if (!gene.CanLowerReserve(ability.def.beuPerTick)) {
            pawn.health.RemoveHediff(this);
            return;
        }

        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond / 2, delta)) return;

        List<Verse.Hediff> hediffs = pawn.health.hediffSet.hediffs
            .Where(h => h.CanBeHealedByInvestiture())
            .ToList();

        if (CheckIfDone(hediffs)) return;

        for (int i = 0; i < hediffs.Count; i++) {
            if (!hediffs[i].TryHealWithInvestiture(ability.gene.currentIdealDisplay)) {
                continue;
            }

            if (hediffs[i].ShouldRemove) {
                hediffs.RemoveAt(i);
            }

            pawn.skills?.Learn(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower, 5);
            CheckIfDone(hediffs);
            return;
        }
    }

    private bool CheckIfDone(List<Verse.Hediff> hediffs) {
        if (hediffs.Count != 0) return false;
        if (!Mathf.Approximately(pawn.health.summaryHealth.SummaryHealthPercent, 1)) return false;

        pawn.health.RemoveHediff(this);
        return true;
    }
}
