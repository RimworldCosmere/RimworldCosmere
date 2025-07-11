using CosmereFramework.Extension;
using RimWorld;
using Verse;

namespace CosmereFramework.Comp.Thing;

public class GiveGeneProperties : CompProperties {
    public GeneDef? geneDef;

    public GiveGeneProperties() {
        compClass = typeof(GiveGene);
    }
}

public class GiveGene : ThingComp {
    public new GiveGeneProperties props => (GiveGeneProperties)base.props;

    public override void PrePostIngested(Pawn ingester) {
        base.PrePostIngested(ingester);
        if (ingester?.genes == null || props.geneDef == null) return;

        ingester.genes.TryAddGene(props.geneDef);
        Messages.Message(
            "CF_GiveGene".Translate(
                    ingester.NameFullColored.Named("PAWN"),
                    props.geneDef.LabelCap.Colorize(ColoredText.GeneColor)
                )
                .Resolve(),
            ingester,
            MessageTypeDefOf.PositiveEvent
        );
    }
}