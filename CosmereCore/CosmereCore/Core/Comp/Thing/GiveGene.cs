using RimWorld;
using Verse;

namespace Cosmere.Core.Comp.Thing;

public class GiveGeneProperties : CompProperties {
    public GeneDef? geneDef;

    public GiveGeneProperties() {
        compClass = typeof(GiveGene);
    }
}

public class GiveGene : ThingComp {
    public GiveGeneProperties Props => (GiveGeneProperties)props;

    public override void PrePostIngested(Pawn ingester) {
        base.PrePostIngested(ingester);
        if (ingester?.genes == null || Props.geneDef == null) return;

        ingester.genes.EnsureGene(Props.geneDef);
        Messages.Message(
            "CC_GiveGene".Translate(
                    ingester.NameFullColored.Named("PAWN"),
                    Props.geneDef.LabelCap.Colorize(ColoredText.GeneColor)
                )
                .Resolve(),
            ingester,
            MessageTypeDefOf.PositiveEvent
        );
    }
}
