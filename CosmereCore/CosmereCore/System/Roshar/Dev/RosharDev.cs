using Cosmere.Core.Need;
using Cosmere.System.Roshar.Util;
using LudeonTK;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Dev;

public static class RosharDev {
    [DebugAction(
        "Cosmere/Roshar",
        "Prepare Dev Pawn",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void PrepareDevPawn(Pawn pawn) {
        if (pawn.genes == null || pawn.story == null) return;

        if (pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Roshar_Trait_RadiantWindrunner)) return;

        pawn.genes.EnsureGene(GeneDefOf.Cosmere_Roshar_Gene_RadiantWindrunner);
        Messages.Message($"Made {pawn.NameFullColored} a Windrunner", pawn, MessageTypeDefOf.PositiveEvent);
        pawn.needs.TryGetNeed<Investiture>().CurLevel += 20000;
    }

    [DebugAction(
        "Cosmere/Roshar",
        "Bond with Spren",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void BondWithSpren(Pawn pawn) {
        RadiantOrderUtility.BondWithSpren(pawn);
    }
}
