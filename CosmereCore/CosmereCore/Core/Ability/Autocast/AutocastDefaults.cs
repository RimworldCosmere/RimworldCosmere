namespace Cosmere.Core.Ability.Autocast;

public static class AutocastDefaults {
    private static readonly Dictionary<string, List<AutocastTrigger>> seeds = new();
    private static readonly HashSet<string> releaseOnStop = [];

    public static IEnumerable<string> SeededAbilityDefNames => seeds.Keys;

    public static void Register(
        string abilityDefName,
        List<AutocastTrigger> defaults,
        bool toggleOffWhenInactive = false) {
        seeds[abilityDefName] = defaults;
        if (toggleOffWhenInactive) releaseOnStop.Add(abilityDefName);
    }

    public static bool HasDefaults(string abilityDefName) {
        return seeds.ContainsKey(abilityDefName);
    }

    public static void ApplyTo(AutocastRule rule) {
        if (!seeds.TryGetValue(rule.AbilityDefName, out List<AutocastTrigger>? template)) return;
        if (rule.Triggers.Count > 0) return;

        rule.ToggleOffWhenInactive = releaseOnStop.Contains(rule.AbilityDefName);
        for (int i = 0; i < template.Count; i++) {
            AutocastTrigger src = template[i];
            rule.Triggers.Add(new AutocastTrigger(src.Kind, src.Comparison, src.Threshold));
        }
    }
}
