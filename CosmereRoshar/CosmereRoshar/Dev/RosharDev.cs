using LudeonTK;
using RimWorld;
using Verse;

namespace Cosmere.Roshar.Dev;

public static class RosharDev {
    [DebugAction(
        "Cosmere/Roshar",
        "Prepare Dev Pawn",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void PrepareDevPawn(Pawn pawn) {
        if (pawn.genes == null) return;
        if (!pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Mistborn)) {
            GeneUtility.AddMistborn(pawn, false, true);
            Messages.Message($"Made {pawn.NameFullColored} a mistborn", pawn, MessageTypeDefOf.PositiveEvent);
        }

        FillAllReserves(pawn);
        GiveAllAllomanticVials(pawn);
    }
}