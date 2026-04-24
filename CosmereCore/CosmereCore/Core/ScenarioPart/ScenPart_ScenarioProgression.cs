using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class ScenPart_ScenarioProgression : ScenPart {
    public ScenarioProgressionDef? progression;

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Defs.Look(ref progression, "progression");
    }

    public override string Summary(Scenario scen) {
        if (progression == null) return "";
        return $"Scenario progression: {progression.label}";
    }
}