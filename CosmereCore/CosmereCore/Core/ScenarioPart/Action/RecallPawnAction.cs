using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

/// <summary>
///     Brings back somebody a previous beat sent away. RemovePawnAction with method "leave"
///     passes a pawn to the world rather than discarding them, so the same person can walk back
///     in later - which is what makes "she has gone to do something and will return" a thing the
///     story can actually say.
/// </summary>
public class RecallPawnAction : ProgressionAction {
    public string pawnName = string.Empty;
    public string? letterText;
    public string? letterTitle;
    public bool optional = true;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        // Already home. A beat that welcomes somebody back should not duplicate them.
        if (comp.FindPawnByName(pawnName) != null) return;

        Pawn? pawn = FindInWorld(pawnName);
        if (pawn == null) {
            if (!optional) Log.Warn($"ScenarioProgression: '{pawnName}' is not in the world to recall.");
            return;
        }

        Map? map = Find.AnyPlayerHomeMap;
        if (map == null) return;

        if (!CellFinder.TryFindRandomEdgeCellWith(
                c => map.reachability.CanReachColony(c) && !c.Fogged(map),
                map,
                CellFinder.EdgeRoadChance_Neutral,
                out IntVec3 cell
            )) {
            return;
        }

        Find.WorldPawns.RemovePawn(pawn);
        if (pawn.Faction != Faction.OfPlayer) pawn.SetFaction(Faction.OfPlayer);
        GenSpawn.Spawn(pawn, cell, map);

        if (letterTitle != null && letterText != null) {
            Find.LetterStack.ReceiveLetter(letterTitle, letterText, LetterDefOf.PositiveEvent, pawn);
        }

        Log.Info($"ScenarioProgression: {pawnName} is back.");
    }

    private static Pawn? FindInWorld(string firstName) {
        List<Pawn> pawns = Find.WorldPawns.AllPawnsAlive;
        for (int i = 0; i < pawns.Count; i++) {
            Pawn pawn = pawns[i];
            if (pawn.Name is NameTriple triple &&
                (triple.First == firstName || triple.Nick == firstName)) {
                return pawn;
            }

            if (pawn.Name is NameSingle single && single.Name.StartsWith(firstName)) return pawn;
        }

        return null;
    }

    public override string? Describe() {
        return "CC_Progression_Effect_Recall".Translate(pawnName.Named("PAWN")).Resolve();
    }
}
