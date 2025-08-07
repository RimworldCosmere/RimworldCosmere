using Cosmere.Foundation;
using Cosmere.Resources;
using Cosmere.Scadrial.Gene;
using Verse;

namespace Cosmere.Scadrial.Feruchemy.Hediff;

public class Gold : HediffWithComps {
    protected bool isTapping => def.Equals(HediffDefOf.Cosmere_Scadrial_Hediff_TapGold);
    protected Feruchemist? gold => pawn.genes.GetFeruchemicGeneForMetal(MetalDefOf.Gold);

    public override void PostMake() {
        base.PostMake();

        if (gold == null) {
            Logger.Error("CS_Error_MissingRequirement".Translate("Gold", "the Gold gene"));
            pawn.health.RemoveHediff(this);
        }
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) return;

        if (!isTapping) return;

        List<Verse.Hediff> hediffs = pawn.health.hediffSet.hediffs
            .Where(h => h.CanBeHealedByInvestiture())
            .ToList();

        foreach (Verse.Hediff hediff in hediffs) {
            if (hediff.TryHealWithInvestiture(Severity)) return;
        }
    }
}