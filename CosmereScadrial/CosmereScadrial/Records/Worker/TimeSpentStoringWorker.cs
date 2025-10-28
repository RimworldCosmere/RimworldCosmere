using Cosmere.Def;
using Cosmere.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.Scadrial.Records.Worker;

public class TimeSpentStoringWorker : RecordWorker {
    private MetalDef? metalCache;

    private MetalDef metal => metalCache ??=
        DefDatabase<MetalDef>.GetNamed(def.defName.Replace("Cosmere_Scadrial_Record_TimeSpentStoring_", ""));

    public override bool ShouldMeasureTimeNow(Pawn? pawn) {
        if (pawn?.genes == null || !pawn.genes.HasFeruchemicGeneForMetal(metal)) {
            return false;
        }

        Feruchemist gene = pawn.genes.GetFeruchemicGeneForMetal(metal)!;

        return gene.isStoring;
    }
}