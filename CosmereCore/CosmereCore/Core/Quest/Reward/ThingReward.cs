using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quest.Reward;

/// <summary>Plain items and silver. Delegates placement to vanilla drop pods.</summary>
public class ThingReward : QuestReward {
    public int count = 1;
    public ThingDef? thingDef;

    public override void Give(QuestBuildContext ctx) {
        if (thingDef == null || ctx.map == null) return;

        Verse.Thing stack = ThingMaker.MakeThing(thingDef);
        stack.stackCount = count;

        DropPodUtility.DropThingsNear(
            DropCellFinder.TradeDropSpot(ctx.map),
            ctx.map,
            new List<Verse.Thing> { stack }
        );
    }

    public override string Describe() {
        return thingDef == null
            ? string.Empty
            : "CC_Quest_Reward_Thing".Translate(thingDef.label.Named("THING"), count.Named("COUNT")).Resolve();
    }

    public override string? ConfigError() {
        if (thingDef == null) return "ThingReward has no thingDef.";
        if (count < 1) return "ThingReward count must be at least 1.";
        return null;
    }
}
