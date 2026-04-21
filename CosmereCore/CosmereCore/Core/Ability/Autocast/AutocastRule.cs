using System.Collections.Generic;
using Verse;

namespace Cosmere.Core.Ability.Autocast;

public sealed class AutocastRule : IExposable {
    public string AbilityDefName = "";
    public bool Enabled = true;
    public float CostCapFraction = 0.5f;
    public int FireCount;
    public List<AutocastTrigger> Triggers = [];

    public void ExposeData() {
        Scribe_Values.Look(ref AbilityDefName, "abilityDefName", "");
        Scribe_Values.Look(ref Enabled, "enabled", true);
        Scribe_Values.Look(ref CostCapFraction, "costCapFraction", 0.5f);
        Scribe_Values.Look(ref FireCount, "fireCount");
        Scribe_Collections.Look(ref Triggers, "triggers", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit && Triggers == null) {
            Triggers = [];
        }
    }
}
