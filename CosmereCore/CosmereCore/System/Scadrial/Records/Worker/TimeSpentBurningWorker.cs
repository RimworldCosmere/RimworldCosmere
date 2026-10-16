using Cosmere.Core.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Records.Worker;

public class TimeSpentBurningWorker : RecordWorker {
    private MetalDef? metalCache;

    private MetalDef Metal => metalCache ??=
        DefDatabase<MetalDef>.GetNamed(def.defName.Replace("Cosmere_Scadrial_Record_TimeSpentBurning_", string.Empty));

    public override bool ShouldMeasureTimeNow(Pawn? pawn) {
        if (pawn?.genes == null || !pawn.genes.HasAllomanticGeneForMetal(Metal)) {
            return false;
        }

        Allomancer gene = pawn.genes.GetAllomanticGeneForMetal(Metal)!;

        return gene.Burning;
    }
}
