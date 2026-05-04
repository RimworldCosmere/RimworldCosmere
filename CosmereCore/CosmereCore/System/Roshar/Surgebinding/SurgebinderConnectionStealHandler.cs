using Cosmere.Core.Comp.Game;
using Cosmere.Core.Framework;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding;

public sealed class SurgebinderConnectionStealHandler : IConnectionStealHandler {
    public void OnConnectionStolen(Pawn donor) {
        if (donor.genes == null) return;
        Surgebinder? surgebinder = donor.genes.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        surgebinder.CurrentIdeal = 0;
        ILoadReferenceable? bondTarget = surgebinder.GetBondTarget();
        if (bondTarget == null) return;

        SpiritWeb.Instance?.SetConnection(donor, bondTarget, 0.1f);
        Verse.Hediff? strainedBond = donor.health.hediffSet.GetFirstHediffOfDef(
            HediffDefOf.Cosmere_Roshar_Hediff_StrainedBond
        );
        if (strainedBond == null) {
            strainedBond = HediffMaker.MakeHediff(
                HediffDefOf.Cosmere_Roshar_Hediff_StrainedBond,
                donor
            );
            donor.health.AddHediff(strainedBond);
        }
    }
}
