using Verse;

namespace Cosmere.Core.Quest.Reward;

/// <summary>
///     Sets a CosmereQuestFlag. This is how Crystal in the Deep unlocks the Pits capstone.
/// </summary>
public class FlagReward : QuestReward {
    public string? flag;

    public override void Give(QuestBuildContext ctx) {
        string? f = flag;
        if (f != null && f.Length > 0) QuestFlagStore.SetFlag(f);
    }

    public override string Describe() {
        return "CC_Quest_Reward_Knowledge".Translate().Resolve();
    }

    public override string? ConfigError() {
        return string.IsNullOrEmpty(flag) ? "FlagReward has no flag." : null;
    }
}
