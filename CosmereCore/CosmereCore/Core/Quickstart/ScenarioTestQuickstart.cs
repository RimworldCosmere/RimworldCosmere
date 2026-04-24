using Cosmere.Core.Settings;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quickstart;

public class ScenarioTestQuickstart : AbstractQuickstart {
    public override TaggedString description =>
        "Generic scenario test. Starts the scenario selected in Core mod settings with no pawn overrides.";

    public override ScenarioDef scenario {
        get {
            string? defName = Mod.GetModSettings<CoreModSettings>().testScenarioDefName;
            if (string.IsNullOrEmpty(defName)) {
                return ScenarioDefOf.Crashlanded;
            }

            ScenarioDef? selected = DefDatabase<ScenarioDef>.GetNamedSilentFail(defName);
            if (selected == null) {
                Logger.Warning(
                    $"ScenarioTestQuickstart: ScenarioDef '{defName}' not found, falling back to Crashlanded"
                );
                return ScenarioDefOf.Crashlanded;
            }

            return selected;
        }
    }
}