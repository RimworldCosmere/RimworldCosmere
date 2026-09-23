using RimWorld;
using Verse;

namespace Cosmere.Core.Quest.Reward;

/// <summary>
///     Progress toward the player's current research project. Expressed as a fraction of one
///     project so the reward scales with tech level rather than becoming worthless late.
/// </summary>
public class ResearchReward : QuestReward {
    public float projectFraction = 0.5f;

    public override void Give(QuestBuildContext ctx) {
        ResearchProjectDef current = Find.ResearchManager.GetProject();
        if (current == null) return;

        Find.ResearchManager.AddProgress(current, current.Cost * projectFraction);
    }

    public override string Describe() {
        return "CC_Quest_Reward_Research".Translate((projectFraction * 100f).ToString("0").Named("PERCENT"))
            .Resolve();
    }

    public override string? ConfigError() {
        return projectFraction <= 0f ? "ResearchReward projectFraction must be positive." : null;
    }
}
