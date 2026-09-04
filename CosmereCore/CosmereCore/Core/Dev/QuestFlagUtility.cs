using System.Collections.Generic;
using Cosmere.Core.Quest;
using Cosmere.Core.Quest.Reward;
using LudeonTK;
using RimWorld;
using Verse;

namespace Cosmere.Core.Dev;

/// <summary>
///     Sets the quest flags that gate later content. Without this, reaching a flag-gated
///     capstone in a test means playing the whole quest that grants the flag first.
/// </summary>
[StaticConstructorOnStartup]
public static class QuestFlagUtility {
    [DebugAction(
        "Cosmere/Core",
        "Set quest flag...",
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void SetQuestFlag() {
        CosmereQuestManager? manager = Current.Game?.GetComponent<CosmereQuestManager>();
        if (manager == null) return;

        List<DebugMenuOption> options = new List<DebugMenuOption>();
        foreach (string flag in KnownFlags()) {
            string label = manager.HasFlag(flag) ? flag + " (already set)" : flag;
            options.Add(
                new DebugMenuOption(
                    label,
                    DebugMenuOptionMode.Action,
                    () => {
                        manager.SetFlag(flag);
                        Log.Info($"Quest flag '{flag}' set.");
                        Messages.Message($"Quest flag '{flag}' set.", MessageTypeDefOf.TaskCompletion, false);
                    }
                )
            );
        }

        if (options.Count == 0) {
            Messages.Message("No quest flags are declared by any quest def.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }

    /// <summary>
    ///     Every flag any quest either grants or requires. Read off the defs so the menu cannot
    ///     drift out of step with the content.
    /// </summary>
    private static List<string> KnownFlags() {
        SortedSet<string> flags = new SortedSet<string>();

        List<CosmereQuestDef> defs = DefDatabase<CosmereQuestDef>.AllDefsListForReading;
        for (int i = 0; i < defs.Count; i++) {
            CosmereQuestDef def = defs[i];

            List<string>? required = def.requiredFlags;
            if (required != null) {
                for (int j = 0; j < required.Count; j++) {
                    flags.Add(required[j]);
                }
            }

            List<QuestReward>? rewards = def.rewards;
            if (rewards == null) continue;
            for (int j = 0; j < rewards.Count; j++) {
                if (rewards[j] is FlagReward reward && reward.flag != null && reward.flag.Length > 0) {
                    flags.Add(reward.flag);
                }
            }
        }

        return new List<string>(flags);
    }
}
