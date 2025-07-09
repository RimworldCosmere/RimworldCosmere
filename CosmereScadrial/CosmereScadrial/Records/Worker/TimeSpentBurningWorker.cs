using CosmereResources.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using RimWorld;
using Verse;

namespace CosmereScadrial.Records.Worker;

public class TimeSpentBurningWorker : RecordWorker {
    private MetalDef? metalCache;

    private MetalDef metal => metalCache ??=
        DefDatabase<MetalDef>.GetNamed(def.defName.Replace("Cosmere_Scadrial_Record_TimeSpentBurning_", ""));

    public override bool ShouldMeasureTimeNow(Pawn? pawn) {
        if (!pawn?.genes?.HasAllomanticGeneForMetal(metal) ?? true) {
            return false;
        }

        Allomancer gene = pawn.genes.GetAllomanticGeneForMetal(metal)!;

        return gene.Burning;
    }
}