using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Allomancy.Comp.Thing;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using RimWorld;
using Verse;
using GeneUtility = CosmereScadrial.Util.GeneUtility;

namespace CosmereScadrial.Thing;

public class AllomanticMetal : AllomanticVial {
    protected override void PostIngested(Pawn ingester) {
        MetallicArtsMetalDef? metal = metals.First();
        if (metal.godMetal) {
            MetalReserves? comp = ingester.GetComp<MetalReserves>();
            if (metal.Equals(MetallicArtsMetalDefOf.Lerasium)) {
                GeneUtility.AddMistborn(ingester, false, true, "ingested Lerasium");
                foreach (MetallicArtsMetalDef? m in DefDatabase<MetallicArtsMetalDef>.AllDefs) {
                    comp.SetReserve(m, MetalReserves.MaxAmount);
                }
            } else if (metal.Equals(MetallicArtsMetalDefOf.Atium)) {
                GeneUtility.AddGene(ingester, metal.GetMistingGene(), false, true);
                comp.SetReserve(metal, MetalReserves.MaxAmount);
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
            $"{ingester.LabelShortCap} downed a bit of: {string.Join(", ", metals.Select(x => x.LabelCap))}.",
            ingester, MessageTypeDefOf.PositiveEvent);
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn) {
        yield break;
    }

    public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(IEnumerable<Pawn> selPawns) {
        yield break;
    }
}