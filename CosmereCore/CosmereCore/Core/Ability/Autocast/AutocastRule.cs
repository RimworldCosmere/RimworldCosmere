using Verse;

namespace Cosmere.Core.Ability.Autocast;

public enum AutocastRuleKind {
    Ability,

    /// <summary>
    ///     Holds a feruchemical dial rather than casting. A dial is a state the pawn stays
    ///     in, not an act they perform once.
    /// </summary>
    FeruchemyDial,
}

// What becomes of a held dial once its triggers stop passing.
public enum AutocastRelease {
    ToIdle,
    Leave,
    ToRest,
}

public sealed class AutocastRule : IExposable {
    public string AbilityDefName = string.Empty;
    public float CostCapFraction = 0.5f;
    public bool Enabled = true;
    public int FireCount;
    public List<AutocastTrigger> Triggers = [];

    public AutocastRuleKind Kind = AutocastRuleKind.Ability;

    // Which metal's dial this rule holds, for FeruchemyDial rules.
    public string MetalDefName = string.Empty;

    // Where the dial sits while the triggers pass, and where it goes after.
    public float ActiveTarget = 25f;
    public AutocastRelease Release = AutocastRelease.ToIdle;
    public float RestTarget = 50f;

    /// <summary>
    ///     Whether to switch a toggled ability back off once triggers stop passing. Off by
    ///     default: some sustained surges are worth keeping up between fights.
    /// </summary>
    public bool ToggleOffWhenInactive;

    /// <summary>
    ///     Whether this rule is the reason the dial sits where it does. Persisted so a save
    ///     made mid-hold releases the dial it set, not one the player moved by hand.
    /// </summary>
    public bool Holding;

    public void ExposeData() {
        Scribe_Values.Look(ref AbilityDefName, "abilityDefName", string.Empty);
        Scribe_Values.Look(ref Enabled, "enabled", true);
        Scribe_Values.Look(ref CostCapFraction, "costCapFraction", 0.5f);
        Scribe_Values.Look(ref FireCount, "fireCount");
        Scribe_Values.Look(ref Kind, "kind");
        Scribe_Values.Look(ref MetalDefName, "metalDefName", string.Empty);
        Scribe_Values.Look(ref ActiveTarget, "activeTarget", 25f);
        Scribe_Values.Look(ref Release, "release");
        Scribe_Values.Look(ref RestTarget, "restTarget", 50f);
        Scribe_Values.Look(ref Holding, "holding");
        Scribe_Values.Look(ref ToggleOffWhenInactive, "toggleOffWhenInactive");
        Scribe_Collections.Look(ref Triggers, "triggers", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit && Triggers == null) {
            Triggers = [];
        }
    }
}
