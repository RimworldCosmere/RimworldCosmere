using CosmereResources.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using RimWorld;
using Verse;

namespace CosmereScadrial.Records.Worker;

public class TimeSpentTappingWorker : RecordWorker {
    private MetalDef? metalCache;

    private MetalDef metal => metalCache ??=
        DefDatabase<MetalDef>.GetNamed(def.defName.Replace("Cosmere_Scadrial_Record_TimeSpentTapping_", ""));

    public override bool ShouldMeasureTimeNow(Pawn? pawn) {
        if (!pawn?.genes?.HasFeruchemicGeneForMetal(metal) ?? true) {
            return false;
        }

        Feruchemist gene = pawn.genes.GetFeruchemicGeneForMetal(metal)!;

        return gene.isTapping;
    }
}