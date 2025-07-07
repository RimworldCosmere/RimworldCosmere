using System.Collections.Generic;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using RimWorld;
using Verse;
using GeneUtility = CosmereScadrial.Util.GeneUtility;

namespace CosmereScadrial.Thing;

public class AllomanticMetal : AllomanticVial {
    public override MetallicArtsMetalDef metal => DefDatabase<MetallicArtsMetalDef>.GetNamed(def.defName);

    protected override void PostIngested(Pawn ingester) {
        if (metal.godMetal) {
            if (metal.Equals(MetallicArtsMetalDefOf.Lerasium)) {
                GeneUtility.AddMistborn(ingester, false, true, "ingested Lerasium");
                ingester.FillAllAllomanticReserves();
            }

            if (metal.Equals(MetallicArtsMetalDefOf.Leratium)) {
                GeneUtility.AddFullFeruchemist(ingester, false, true, "ingested Leratium");
            }

            if (metal.Equals(MetallicArtsMetalDefOf.Atium)) {
                GeneUtility.AddGene(ingester, metal.GetMistingGene(), false, true);
                ingester.FillAllomanticReserves(metal);
            }

            if (metal.Equals(MetallicArtsMetalDefOf.LerasiumAlloy)) {
                MetallicArtsMetalDef? stuffMetal = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(Stuff.defName);
                GeneUtility.AddGene(ingester, stuffMetal.GetMistingGene(), false, true);
                ingester.FillAllomanticReserves(stuffMetal);
            }

            if (metal.Equals(MetallicArtsMetalDefOf.LeratiumAlloy)) {
                MetallicArtsMetalDef? stuffMetal = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(Stuff.defName);
                GeneUtility.AddGene(ingester, stuffMetal.GetFerringGene(), false, true);
            }

            Find.LetterStack.ReceiveLetter(
                "CS_BurnedGodMetal".Translate(metal.LabelCap.Named("METAL")),
                $"CS_BurnedGodMetal_{metal.LabelCap}".Translate(ingester.NameFullColored.Named("PAWN")).Resolve(),
                LetterDefOf.PositiveEvent,
                ingester
            );

            return;
        }

        Messages.Message(
            "CS_IngestedThing".Translate(ingester.NameFullColored.Named("PAWN"), metal.coloredLabel.Named("THING")),
            ingester,
            MessageTypeDefOf.PositiveEvent
        );
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn) {
        yield break;
    }

    public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(IEnumerable<Pawn> selPawns) {
        yield break;
    }
}