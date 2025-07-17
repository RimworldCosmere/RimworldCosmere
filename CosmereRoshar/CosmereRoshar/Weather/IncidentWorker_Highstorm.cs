using RimWorld;
using Verse;

namespace CosmereRoshar {
    public class IncidentWorker_Highstorm : IncidentWorker {
        protected override bool TryExecuteWorker(IncidentParms parms) {
            Map map = parms.target as Map ?? Find.AnyPlayerHomeMap;
            if (map == null) {
                Log.Warning("Highstorm incident: No valid map found.");
                return false;
            }

            // Read from this IncidentDef
            IncidentDef incDef = this.def;

            // Show the letter using the Def's label, text, and letterDef
            SendStandardLetter(
                incDef.letterLabel,
                incDef.letterText,
                incDef.letterDef ?? LetterDefOf.ThreatBig,
                parms, null
            );

            // Start the Highstorm condition
            GameConditionDef stormDef = DefDatabase<GameConditionDef>.GetNamed("whtwl_HighstormCondition", true);
            var ext = stormDef.GetModExtension<HighstormExtension>();
            int stormDuration = ext.stormDuration; 
            GameCondition storm = GameConditionMaker.MakeCondition(stormDef, stormDuration);
            map.gameConditionManager.RegisterCondition(storm);

            // Transition to the custom weather from XML
            WeatherDef highstormWeather = DefDatabase<WeatherDef>.GetNamed("whtwl_HighstormWeather", false);
            if (highstormWeather != null)
                map.weatherManager.TransitionTo(highstormWeather);

            return true;
        }
    }
}
