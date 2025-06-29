using System.Collections.Generic;
using System.Linq;
using CosmereResources.DefModExtension;
using CosmereScadrial.Allomancy.Comp.Thing;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Util;
using RimWorld;
using Verse;

namespace CosmereScadrial.Thing;

public class AllomanticVial : ThingWithComps {
    public List<MetallicArtsMetalDef> metals => def.GetModExtension<MetalsLinked>().Metals
        .Select(x => x.ToMetallicArts()).ToList();

    public bool IsForMetal(MetallicArtsMetalDef metal, bool singleMetal = true) {
        if (singleMetal && metals.Count > 1) return false;

        return metals.Contains(metal);
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn) {
        yield break;
    }

    public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(IEnumerable<Pawn> selPawns) {
        yield break;
    }

    protected override void PostIngested(Pawn ingester) {
        base.PostIngested(ingester);

        metals.ForEach(metal => {
            AllomancyUtility.AddMetalReserve(ingester, metal, MetalReserves.MaxAmount); // or adjust amount
        });
        Messages.Message(
            $"{ingester.LabelShortCap} downed a vial containing: {string.Join(", ", metals.Select(x => x.LabelCap))}.",
            ingester, MessageTypeDefOf.PositiveEvent);
    }
}