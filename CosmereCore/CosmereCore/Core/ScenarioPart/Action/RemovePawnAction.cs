using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

public class RemovePawnAction : ProgressionAction {
    public string? letterText;
    public string? letterTitle;
    public string method = "vanish";

    /// <summary>
    ///     Set when the pawn being gone is a normal outcome rather than a broken def, so the
    ///     miss logs quietly. A story beat that resolves the same way whether or not its pawn
    ///     is still around should not read as a fault every time it takes that path.
    /// </summary>
    public bool optional;

    public string pawnName = string.Empty;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            if (optional) {
                Logger.Verbose($"ScenarioProgression: '{pawnName}' is already gone, nothing to remove.");
            } else {
                Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for RemovePawn");
            }

            return;
        }

        if (letterTitle != null && letterText != null) {
            LetterDef letterDef = method == "death" ? LetterDefOf.Death : LetterDefOf.NegativeEvent;
            Find.LetterStack.ReceiveLetter(letterTitle, letterText, letterDef, pawn);
        }

        switch (method) {
            case "death":
                pawn.Kill(null);
                break;
            case "leave":
                // DeSpawn throws on a pawn who is not on a map, and FindPawnByName can now hand
                // back one who is away with a caravan.
                if (pawn.Spawned) pawn.DeSpawn();
                pawn.holdingOwner?.Remove(pawn);
                Find.WorldPawns.PassToWorld(pawn);
                break;
            default:
                if (pawn.Spawned) pawn.DeSpawn();
                pawn.holdingOwner?.Remove(pawn);
                Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                break;
        }
    }
}
