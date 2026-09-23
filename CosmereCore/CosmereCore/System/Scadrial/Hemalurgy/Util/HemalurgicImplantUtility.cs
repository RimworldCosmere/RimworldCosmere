using Cosmere.Core.Comp.Thing;
using Cosmere.System.Scadrial.Hemalurgy.Hediff;
using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy;

public static class HemalurgicImplantUtility {
    public static void ApplyHemalurgicEffect(Pawn target, ImplantedSpikeData spike) {
        if (HemalurgicConstants.IsAllomanticSteal(spike.stealType) ||
            HemalurgicConstants.IsFeruchemicSteal(spike.stealType) ||
            spike.stealType == HemalurgicStealType.AnyPower) {
            GrantGene(target, spike.stolenDefName);
        } else if (spike.stealType == HemalurgicStealType.AllAbilities) {
            for (int i = 0; i < spike.stolenDefNames.Count; i++) {
                GrantGene(target, spike.stolenDefNames[i]);
            }
        } else if (spike.stealType == HemalurgicStealType.ConnectionIdentity) {
            if (!target.IsSnapped()) {
                SnapUtility.Snap(target, "CS_Hemalurgy_SnappedByConnection");
            }
        }
    }

    public static void GrantGene(Pawn target, string geneDefName) {
        if (geneDefName.NullOrEmpty()) return;
        GeneDef? geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(geneDefName);
        if (geneDef == null) return;
        if (target.genes.HasActiveGene(geneDef)) return;
        target.genes.AddGene(geneDef, true);
    }

    public static void GrantStoredInvestiture(Pawn target, float amount) {
        if (amount <= 0f) return;
        InvestitureHolder? holder = target.TryGetComp<InvestitureHolder>();
        if (holder == null) return;
        holder.currentInvestitureSelf += amount;
    }

    public static void RemoveDrabIfPresent(Pawn target) {
        Verse.Hediff? drab = target.health.hediffSet.GetFirstHediffOfDef(HemalurgicDefOf.Cosmere_Scadrial_Hediff_Drab);
        if (drab != null) {
            target.health.RemoveHediff(drab);
        }
    }

    public static void AddToUnifiedHediff(Pawn target, ImplantedSpikeData spikeData, BodyPartRecord? part) {
        HemalurgicSpikes? hediff = (HemalurgicSpikes?)target.health.hediffSet.GetFirstHediffOfDef(
            HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
        );
        if (hediff == null) {
            hediff = (HemalurgicSpikes)HediffMaker.MakeHediff(
                HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes,
                target,
                part
            );
            target.health.AddHediff(hediff, part);
        }

        hediff.AddSpike(spikeData);
    }

    public static void UpdateRuinsInfluence(Pawn target) {
        HemalurgicSpikes? spikesHediff = (HemalurgicSpikes?)target.health.hediffSet.GetFirstHediffOfDef(
            HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
        );
        int spikeCount = spikesHediff?.spikeCount ?? 0;

        RuinsInfluence? ruinsInfluence = (RuinsInfluence?)target.health.hediffSet.GetFirstHediffOfDef(
            HemalurgicDefOf.Cosmere_Scadrial_Hediff_RuinsInfluence
        );
        if (ruinsInfluence == null && spikeCount > 0) {
            Verse.Hediff hediff = HediffMaker.MakeHediff(
                HemalurgicDefOf.Cosmere_Scadrial_Hediff_RuinsInfluence,
                target
            );
            target.health.AddHediff(hediff);
            ruinsInfluence = (RuinsInfluence)hediff;
        }

        ruinsInfluence?.UpdateSpikeCount(spikeCount);
    }
}
