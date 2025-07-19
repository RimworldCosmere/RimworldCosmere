using RimWorld;
using Verse;

namespace CosmereRoshar.Incident.Worker;

public class Highstorm : IncidentWorker {
    protected override bool TryExecuteWorker(IncidentParms parms) {
        Map map = parms.target as Map ?? Find.AnyPlayerHomeMap;
        if (map == null) {
            Log.Warning("Highstorm incident: No valid map found.");
            return false;
        }

        // Read from this IncidentDef
        IncidentDef incDef = def;

        // Show the letter using the Def's label, text, and letterDef
        SendStandardLetter(
            incDef.letterLabel,
            incDef.letterText,
            incDef.letterDef ?? LetterDefOf.ThreatBig,
            parms,
            null
        );

        // Start the Highstorm condition
        GameConditionDef stormDef = DefDatabase<GameConditionDef>.GetNamed("Cosmere_Roshar_HighstormCondition");
        ModExtension.Highstorm? ext = stormDef.GetModExtension<ModExtension.Highstorm>();
        int stormDuration = ext.stormDuration;
        RimWorld.GameCondition storm = GameConditionMaker.MakeCondition(stormDef, stormDuration);
        map.gameConditionManager.RegisterCondition(storm);

        // Transition to the custom weather from XML
        WeatherDef highstormWeather = DefDatabase<WeatherDef>.GetNamed("Cosmere_Roshar_HighstormWeather", false);
        if (highstormWeather != null) {
            map.weatherManager.TransitionTo(highstormWeather);
        }

        return true;
    }
}