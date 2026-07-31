using Cosmere.Core.Quest;
using Cosmere.Core.Quest.Reward;
using Verse;

namespace Cosmere.System.Scadrial.Quest;

/// <summary>
///     The payoff branch on The First Bead. Presented once the bead is home, not at offer
///     time, because the choice only makes sense with the bead in hand.
/// </summary>
public class LerasiumChoiceReward : QuestReward {
    public override void Give(QuestBuildContext ctx) {
        Find.WindowStack.Add(new Dialog_LerasiumChoice(ctx));
    }

    public override string Describe() {
        return "CS_Quest_Reward_FirstBead".Translate().Resolve();
    }
}
