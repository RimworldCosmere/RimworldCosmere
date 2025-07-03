using System.Collections.Generic;
using CosmereScadrial.Allomancy.Comp.Thing;
using CosmereScadrial.Def;
using CosmereScadrial.Util;
using RimWorld;
using Verse;

namespace CosmereScadrial.Thing;

public class AllomanticVial : ThingWithComps {
    public virtual MetallicArtsMetalDef metal => DefDatabase<MetallicArtsMetalDef>.GetNamed(Stuff.defName);

    public bool IsForMetal(MetallicArtsMetalDef metalCheck) {
        return metal.Equals(metalCheck);
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn) {
        yield break;
    }

    public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(IEnumerable<Pawn> selPawns) {
        yield break;
    }

    protected override void PostIngested(Pawn ingester) {
        base.PostIngested(ingester);


        AllomancyUtility.AddMetalReserve(ingester, metal, MetalReserves.MaxAmount); // or adjust amount

        Messages.Message(
            "CS_IngestedVial".Translate(ingester.NameFullColored.Named("PAWN"), metal.coloredLabel.Named("THING")),
            ingester,
            MessageTypeDefOf.PositiveEvent
        );
    }
}