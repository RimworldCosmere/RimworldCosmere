using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class RemovePawnAction : ProgressionAction {
    public string pawnName = "";
    public string method = "vanish";
    public string? letterTitle;
    public string? letterText;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for RemovePawn");
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
                pawn.DeSpawn();
                Find.WorldPawns.PassToWorld(pawn, RimWorld.Planet.PawnDiscardDecideMode.Decide);
                break;
            default:
                pawn.DeSpawn();
                Find.WorldPawns.PassToWorld(pawn, RimWorld.Planet.PawnDiscardDecideMode.Discard);
                break;
        }
    }
}
