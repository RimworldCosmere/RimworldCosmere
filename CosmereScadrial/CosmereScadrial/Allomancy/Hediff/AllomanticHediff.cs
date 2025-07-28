using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.Scadrial.Def;
using Cosmere.Scadrial.Gene;
using Cosmere.Scadrial.Utility;
using Verse;

namespace Cosmere.Scadrial.Allomancy.Hediff;

public class AllomanticHediff(HediffDef hediffDef, Pawn pawn, IAbility<Allomancer, IHediff<Allomancer>> ability)
    : AbstractHediff<Allomancer>(hediffDef, pawn, ability) {
    public MetallicArtsMetalDef metal => gene.metal;

    public override void TickInterval(int delta) {
        SurgeChargeHediff? surge = AllomancyUtility.GetSurgeBurn(pawn);
        if (def.defName != surge?.def.defName && surge != null && severityCalculator != null) {
            surge.Burn(
                severityCalculator.RecalculateSeverity,
                GenTicks.TicksPerRealSecond,
                () => {
                    pawn.genes.GetAllomanticGenes()
                        .Where(g => g.Burning)
                        .ToList()
                        .ForEach(g => g.BurnTickInterval());
                    severityCalculator.RecalculateSeverity();
                }
            );
        }

        base.TickInterval(delta);
    }

    protected override bool IsInvestitureShield() {
        return def.Equals(HediffDefOf.Cosmere_Scadrial_Hediff_InvestitureShield);
    }
}