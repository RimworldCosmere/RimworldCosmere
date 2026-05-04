using Cosmere.System.Scadrial.Def;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Thing;

public class AllomanticVial : ThingWithComps {
    private MetallicArtsMetalDef? cachedMetal;

    public virtual MetallicArtsMetalDef? metal =>
        cachedMetal ??= DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(Stuff?.defName);

    public bool IsForMetal(MetallicArtsMetalDef metalCheck) {
        return metal?.Equals(metalCheck) ?? false;
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn) {
        yield break;
    }

    public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(IEnumerable<Pawn> selPawns) {
        yield break;
    }

    protected override void PostIngested(Pawn ingester) {
        base.PostIngested(ingester);

        if (metal == null) return;

        ingester.genes.GetAllomanticGeneForMetal(metal)?.AddToReserve(ScadrialMetallurgyConstants.VialMetalAmount);

        ingester.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedVial);

        Messages.Message(
            "CS_IngestedVial".Translate(ingester.NameFullColored.Named("PAWN"), metal.coloredLabel.Named("THING")),
            ingester,
            MessageTypeDefOf.PositiveEvent
        );
    }
}