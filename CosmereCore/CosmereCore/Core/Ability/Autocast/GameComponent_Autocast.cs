using Verse;

namespace Cosmere.Core.Ability.Autocast;

public sealed class GameComponent_Autocast : GameComponent {
    private Dictionary<int, List<AutocastRule>> rulesByPawnId = [];

    public GameComponent_Autocast(Game game) { }

    public static GameComponent_Autocast Get() {
        return Current.Game.GetComponent<GameComponent_Autocast>();
    }

    public List<AutocastRule> GetOrCreateRules(Pawn pawn) {
        if (!rulesByPawnId.TryGetValue(pawn.thingIDNumber, out List<AutocastRule>? list)) {
            list = [];
            rulesByPawnId[pawn.thingIDNumber] = list;
        }

        return list;
    }

    /// <summary>
    ///     Every rule a pawn holds against one target. A metal or an ability can carry several,
    ///     each with its own conditions and its own action.
    /// </summary>
    public List<AutocastRule> RulesFor(Pawn pawn, AutocastRuleKind kind, string targetId) {
        List<AutocastRule> matches = [];
        List<AutocastRule> all = GetOrCreateRules(pawn);
        for (int i = 0; i < all.Count; i++) {
            if (all[i].Kind != kind) continue;
            if (TargetOf(all[i]) != targetId) continue;

            matches.Add(all[i]);
        }

        return matches;
    }

    public AutocastRule AddRule(Pawn pawn, AutocastRuleKind kind, string targetId) {
        AutocastRule fresh = new AutocastRule { Kind = kind };
        if (kind == AutocastRuleKind.FeruchemyDial) fresh.MetalDefName = targetId;
        else fresh.AbilityDefName = targetId;

        GetOrCreateRules(pawn).Add(fresh);
        return fresh;
    }

    public void RemoveRule(Pawn pawn, AutocastRule rule) {
        GetOrCreateRules(pawn).Remove(rule);
    }

    public static string TargetOf(AutocastRule rule) {
        return rule.Kind == AutocastRuleKind.FeruchemyDial ? rule.MetalDefName : rule.AbilityDefName;
    }

    /// <summary>
    ///     The seeded rule for an ability, made on first sight. Players can add more beside it;
    ///     this only ever returns the first.
    /// </summary>
    public AutocastRule GetOrCreateRule(Pawn pawn, string abilityDefName) {
        List<AutocastRule> list = GetOrCreateRules(pawn);
        for (int i = 0; i < list.Count; i++) {
            if (list[i].Kind != AutocastRuleKind.Ability) continue;
            if (list[i].AbilityDefName == abilityDefName) return list[i];
        }

        AutocastRule fresh = new AutocastRule { AbilityDefName = abilityDefName };
        AutocastDefaults.ApplyTo(fresh);
        list.Add(fresh);
        return fresh;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref rulesByPawnId, "rulesByPawnId", LookMode.Value, LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit && rulesByPawnId == null) {
            rulesByPawnId = [];
        }
    }
}
