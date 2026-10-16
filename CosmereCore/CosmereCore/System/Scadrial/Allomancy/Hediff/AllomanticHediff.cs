using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.Allomancy.Hediff;

public class AllomanticHediff : AbstractHediff<Allomancer> {
    public AllomanticHediff() { }

    public AllomanticHediff(HediffDef hediffDef, Pawn pawn, IAbility<Allomancer, IHediff<Allomancer>> ability) : base(
        hediffDef,
        pawn,
        ability
    ) { }

    public MetallicArtsMetalDef Metal => Gene.Metal;

    public override void TickInterval(int delta) {
        SurgeChargeHediff? surge = AllomancyUtility.FindSurgeChargeHediff(pawn);
        if (surge != null && SeverityCalculator != null && def.defName != surge.def.defName) {
            surge.Burn(
                SeverityCalculator.RecalculateSeverity,
                GenTicks.TicksPerRealSecond,
                () => {
                    List<Allomancer> genes = pawn.genes.GetAllomanticGenes();
                    for (int i = 0; i < genes.Count; i++) {
                        if (genes[i].Burning) {
                            genes[i].BurnTickInterval();
                        }
                    }

                    SeverityCalculator.RecalculateSeverity();
                }
            );
        }

        base.TickInterval(delta);
    }

    protected override bool IsInvestitureShield() {
        return def.Equals(HediffDefOf.Cosmere_Scadrial_Hediff_InvestitureShield);
    }
}
