using Cosmere.Resources.Def;
using Cosmere.Scadrial.Extension;
using Cosmere.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.Scadrial.Records.Worker;

public class TimeSpentBurningWorker : RecordWorker {
    private MetalDef? metalCache;

    private MetalDef metal => metalCache ??=
        DefDatabase<MetalDef>.GetNamed(def.defName.Replace("Cosmere_Scadrial_Record_TimeSpentBurning_", ""));

    public override bool ShouldMeasureTimeNow(Pawn? pawn) {
        if (pawn?.genes == null || !pawn.genes.HasAllomanticGeneForMetal(metal)) {
            return false;
        }

        Allomancer gene = pawn.genes.GetAllomanticGeneForMetal(metal)!;

        return gene.Burning;
    }
}