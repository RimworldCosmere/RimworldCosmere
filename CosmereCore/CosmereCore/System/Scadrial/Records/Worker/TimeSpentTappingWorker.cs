using Cosmere.Core.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Records.Worker;

public class TimeSpentTappingWorker : RecordWorker {
    private MetalDef? metalCache;

    private MetalDef Metal => metalCache ??=
        DefDatabase<MetalDef>.GetNamed(def.defName.Replace("Cosmere_Scadrial_Record_TimeSpentTapping_", string.Empty));

    public override bool ShouldMeasureTimeNow(Pawn? pawn) {
        if (pawn?.genes == null || !pawn.genes.HasFeruchemicGeneForMetal(Metal)) {
            return false;
        }

        Feruchemist gene = pawn.genes.GetFeruchemicGeneForMetal(Metal)!;

        return gene.IsTapping;
    }
}
