using System.Linq;
using CosmereScadrial.Comp.Thing;
using CosmereScadrial.Def;
using RimWorld;
using Verse;
using GeneUtility = CosmereScadrial.Util.GeneUtility;

namespace CosmereScadrial.Thing;

public class AllomanticMetal : AllomanticVial {
    protected override void PostIngested(Pawn ingester) {
        MetallicArtsMetalDef? metal = metals.First();
        if (metal.defName == MetallicArtsMetalDefOf.Lerasium.defName) {
            GeneUtility.AddMistborn(ingester, false, true, "ingested Lerasium");
            MetalReserves? comp = ingester.GetComp<MetalReserves>();
            foreach (MetallicArtsMetalDef? m in DefDatabase<MetallicArtsMetalDef>.AllDefs) {
                comp.SetReserve(m, MetalReserves.MaxAmount);
            }

            Find.LetterStack.ReceiveLetter(
                "Pawn burned Lerasium!",
                $"{ingester.NameFullColored} consumes the god metal. In an instant, their spirit web reshapes — all paths of Allomancy now lie open.",
                LetterDefOf.PositiveEvent,
                ingester
            );
            return;
        }

        base.PostIngested(ingester);
    }
}