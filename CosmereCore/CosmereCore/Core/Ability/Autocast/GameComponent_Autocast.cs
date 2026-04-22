using System.Collections.Generic;
using Verse;

namespace Cosmere.Core.Ability.Autocast;

public sealed class GameComponent_Autocast : GameComponent {
    private Dictionary<int, List<AutocastRule>> rulesByPawnId = [];

    public GameComponent_Autocast(Game game) { }

    public static GameComponent_Autocast Get() {
        return Current.Game.GetComponent<GameComponent_Autocast>();
    }

    public List<AutocastRule> RulesFor(Pawn pawn) {
        if (!rulesByPawnId.TryGetValue(pawn.thingIDNumber, out List<AutocastRule>? list)) {
            list = [];
            rulesByPawnId[pawn.thingIDNumber] = list;
        }
        return list;
    }

    public AutocastRule GetOrCreateRule(Pawn pawn, string abilityDefName) {
        List<AutocastRule> list = RulesFor(pawn);
        for (int i = 0; i < list.Count; i++) {
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
