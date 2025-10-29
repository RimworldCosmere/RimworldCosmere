using Cosmere.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Records.Worker;

public class TimeSpentTappingWorker : RecordWorker {
    private MetalDef? metalCache;

    private MetalDef metal => metalCache ??=
        DefDatabase<MetalDef>.GetNamed(def.defName.Replace("Cosmere_Scadrial_Record_TimeSpentTapping_", ""));

    public override bool ShouldMeasureTimeNow(Pawn? pawn) {
        if (pawn?.genes == null || !pawn.genes.HasFeruchemicGeneForMetal(metal)) {
            return false;
        }

        Feruchemist gene = pawn.genes.GetFeruchemicGeneForMetal(metal)!;

        return gene.isTapping;
    }
}