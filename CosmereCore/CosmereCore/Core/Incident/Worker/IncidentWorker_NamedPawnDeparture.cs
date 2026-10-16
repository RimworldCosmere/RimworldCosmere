using Cosmere.Core.DefModExtension;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.Incident.Worker;

public class IncidentWorker_NamedPawnDeparture : IncidentWorker {
    protected override bool TryExecuteWorker(IncidentParms parms) {
        Map? map = parms.target as Map ?? Find.AnyPlayerHomeMap;
        if (map == null) return false;

        NamedPawnIncidentConfig? config =
            def.GetModExtension<NamedPawnIncidentConfig>();
        if (config?.pawn?.firstName == null) {
            Log.Warn($"NamedPawnDeparture: No pawn name configured on IncidentDef '{def.defName}'");
            return false;
        }

        Pawn? pawn = FindPawnByName(map, config.pawn.firstName);
        if (pawn == null) {
            Log.Warn($"NamedPawnDeparture: Pawn '{config.pawn.firstName}' not found on map");
            return false;
        }

        SendStandardLetter(
            def.letterLabel.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn),
            def.letterText.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn),
            def.letterDef ?? LetterDefOf.NegativeEvent,
            parms,
            pawn
        );

        string method = config.departureMethod ?? "leave";
        switch (method) {
            case "death":
                pawn.Kill(null);
                break;
            case "vanish":
                pawn.DeSpawn();
                Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                break;
            default:
                pawn.DeSpawn();
                Find.WorldPawns.PassToWorld(pawn);
                break;
        }

        return true;
    }

    private static Pawn? FindPawnByName(Map map, string firstName) {
        List<Pawn> colonists = map.mapPawns.FreeColonists;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn pawn = colonists[i];
            if (pawn.Name is NameTriple triple && triple.First == firstName) return pawn;
            if (pawn.Name is NameSingle single && single.Name.StartsWith(firstName)) return pawn;
        }

        return null;
    }
}
