using RimWorld;
using Verse;

namespace Cosmere.Core.Quest.Reward;

/// <summary>
///     Shifts a faction's standing with the colony. For quests whose payoff is who now owes you
///     rather than what landed in the stockpile.
/// </summary>
public class GoodwillReward : QuestReward {
    public FactionDef? faction;
    public int goodwill;

    public override void Give(QuestBuildContext ctx) {
        if (faction == null) return;

        Faction? target = Find.FactionManager.FirstFactionOfDef(faction);
        if (target == null) {
            Log.Warn($"GoodwillReward on {ctx.def?.defName}: {faction.defName} is not present in this world.");
            return;
        }

        target.TryAffectGoodwillWith(Faction.OfPlayer, goodwill);
    }

    public override string Describe() {
        if (faction == null) return string.Empty;
        return "CC_Quest_Reward_Goodwill".Translate(
            faction.label.Named("FACTION"),
            goodwill.Named("AMOUNT")
        ).Resolve();
    }

    public override string? ConfigError() {
        if (faction == null) return "GoodwillReward has no faction.";
        if (goodwill == 0) return "GoodwillReward goodwill is zero, so it would do nothing.";
        return null;
    }
}
