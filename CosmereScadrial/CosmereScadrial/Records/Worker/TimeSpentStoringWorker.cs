using CosmereResources.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using RimWorld;
using Verse;

namespace CosmereScadrial.Records.Worker;

public class TimeSpentStoringWorker : RecordWorker {
    private Feruchemist? gene;
    private MetalDef? metalCache;

    private MetalDef metal => metalCache ??=
        DefDatabase<MetalDef>.GetNamed(def.defName.Replace("Cosmere_Scadrial_Record_TimeSpentStoring_", ""));

    public override bool ShouldMeasureTimeNow(Pawn? pawn) {
        if (!pawn?.genes?.HasFeruchemicGeneForMetal(metal) ?? false) {
            return false;
        }

        gene ??= pawn?.genes?.GetFeruchemicGeneForMetal(metal);

        return gene?.isStoring ?? false;
    }
}