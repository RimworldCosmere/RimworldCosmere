using Cosmere.Core.Need;
using Cosmere.Framework.Extension;
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

        if (!pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Roshar_Trait_RadiantWindrunner)) {
            pawn.genes.TryAddGene(GeneDefOf.Cosmere_Roshar_Gene_RadiantWindrunner);
            Messages.Message($"Made {pawn.NameFullColored} a Windrunner", pawn, MessageTypeDefOf.PositiveEvent);
            pawn.needs.TryGetNeed<Investiture>().CurLevel += 20000;
        }
    }
}