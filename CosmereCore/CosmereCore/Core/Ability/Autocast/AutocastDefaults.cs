namespace Cosmere.Core.Ability.Autocast;

public static class AutocastDefaults {
    private static readonly Dictionary<string, List<AutocastTrigger>> seeds =
        new Dictionary<string, List<AutocastTrigger>> {
            ["Cosmere_Roshar_Ability_Heal"] = [
                new AutocastTrigger(AutocastTriggerKind.HealthPercent, AutocastComparison.LessThan, 0.6f),
                new AutocastTrigger(AutocastTriggerKind.ReservePercent, AutocastComparison.GreaterThan, 0.1f),
            ],
            ["Cosmere_Scadrial_Ability_Pewter"] = [
                new AutocastTrigger(AutocastTriggerKind.Drafted, AutocastComparison.EqualTo, 0f),
            ],
            ["Cosmere_Scadrial_Ability_Tin"] = [
                new AutocastTrigger(AutocastTriggerKind.Drafted, AutocastComparison.EqualTo, 0f),
            ],
            ["Cosmere_Scadrial_Ability_CompoundGold"] = [
                new AutocastTrigger(AutocastTriggerKind.HealthPercent, AutocastComparison.LessThan, 0.4f),
            ],
        };

    public static IEnumerable<string> SeededAbilityDefNames => seeds.Keys;

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