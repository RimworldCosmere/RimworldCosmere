using System.Collections.Generic;
using System.Linq;
using CosmereFramework;
using CosmereResources;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using Verse;

namespace CosmereScadrial.Feruchemy.Hediff;

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

        List<Hediff_Injury> injuries = pawn.health.hediffSet.hediffs
            .OfType<Hediff_Injury>()
            .Where(x => x.CanHealNaturally() && !x.IsPermanent())
            .ToList();

        foreach (Hediff_Injury injury in injuries) {
            injury.Heal(Severity);
        }
    }
}