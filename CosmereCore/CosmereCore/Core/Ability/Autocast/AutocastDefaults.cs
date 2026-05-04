namespace Cosmere.Core.Ability.Autocast;

public static class AutocastDefaults {
    private static readonly Dictionary<string, List<AutocastTrigger>> seeds = new();

    public static IEnumerable<string> SeededAbilityDefNames => seeds.Keys;

    public static void Register(string abilityDefName, List<AutocastTrigger> defaults) {
        seeds[abilityDefName] = defaults;
    }

    public static bool HasDefaults(string abilityDefName) {
        return seeds.ContainsKey(abilityDefName);
    }

    public static void ApplyTo(AutocastRule rule) {
        if (!seeds.TryGetValue(rule.AbilityDefName, out List<AutocastTrigger>? template)) return;
        if (rule.Triggers.Count > 0) return;
        for (int i = 0; i < template.Count; i++) {
            AutocastTrigger src = template[i];
            rule.Triggers.Add(new AutocastTrigger(src.Kind, src.Comparison, src.Threshold));
        }
    }
}
