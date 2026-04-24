using Cosmere.System.Roshar.Comp.Map;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Incident.Worker;

public class GemheartHunt : IncidentWorker {
    protected override bool CanFireNowSub(IncidentParms parms) {
        Map? map = parms.target as Map;
        if (map == null) return false;

        GemheartExpeditionManager? manager = map.GetComponent<GemheartExpeditionManager>();
        if (manager == null) return false;
        if (!manager.CanStartHunt) return false;

        return map.mapPawns.FreeColonistsCount >= 3;
    }

    protected override bool TryExecuteWorker(IncidentParms parms) {
        Map? map = parms.target as Map ?? Find.AnyPlayerHomeMap;
        if (map == null) return false;

        GemheartExpeditionManager? manager = map.GetComponent<GemheartExpeditionManager>();
        if (manager == null || !manager.CanStartHunt) return false;

        ChoiceLetter_GemheartHunt letter =
            (ChoiceLetter_GemheartHunt)LetterMaker.MakeLetter(def.letterLabel, def.letterText, def.letterDef);
        letter.map = map;
        Find.LetterStack.ReceiveLetter(letter);

        return true;
    }
}