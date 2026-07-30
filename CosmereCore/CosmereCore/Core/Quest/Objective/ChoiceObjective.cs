using System.Collections.Generic;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Asks the player to pick a branch when this stage starts. The chosen key is passed on
///     the completion signal as CHOICE.
/// </summary>
public class ChoiceObjective : QuestObjective {
    public List<QuestChoiceOption>? options;

    public override void AddParts(RimWorld.Quest quest, string inSignal, string outSignal, QuestBuildContext ctx) {
        QuestPart_CosmereChoice choice = new QuestPart_CosmereChoice {
            quest = quest,
            options = options,
            inSignalEnable = inSignal,
            outSignalsCompleted = new List<string> { outSignal },
        };
        quest.AddPart(choice);
    }

    public override string? ConfigError() {
        List<QuestChoiceOption>? opts = options;
        if (opts == null || opts.Count < 2) return "ChoiceObjective needs at least two options.";

        for (int i = 0; i < opts.Count; i++) {
            if (string.IsNullOrEmpty(opts[i].key)) return $"ChoiceObjective option {i} has no key.";
            if (string.IsNullOrEmpty(opts[i].labelKey)) return $"ChoiceObjective option {i} has no labelKey.";
            if (string.IsNullOrEmpty(opts[i].tipKey)) return $"ChoiceObjective option {i} has no tipKey.";
        }

        return null;
    }
}
