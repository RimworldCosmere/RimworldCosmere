using System.Collections.Generic;
using Cosmere.Core.Quest.Objective;
using RimWorld;

namespace Cosmere.Core.Quest;

/// <summary>
///     Reads which branch of a quest the player took. The choice is recorded on the quest's
///     own QuestPart_CosmereChoice rather than threaded through signals, so any later part or
///     reward can ask without the builder having to fork the signal graph.
/// </summary>
public static class QuestBranch {
    public static string? ChosenKey(RimWorld.Quest? quest) {
        if (quest == null) return null;

        List<QuestPart> parts = quest.PartsListForReading;
        for (int i = 0; i < parts.Count; i++) {
            if (parts[i] is QuestPart_CosmereChoice choice) return choice.chosenKey;
        }

        return null;
    }

    /// <summary>
    ///     True when something tagged for one branch should run. An untagged thing always
    ///     runs; a tagged one runs only on its own branch.
    /// </summary>
    public static bool Matches(string? afterChoice, RimWorld.Quest? quest) {
        if (afterChoice == null || afterChoice.Length == 0) return true;
        return ChosenKey(quest) == afterChoice;
    }
}
