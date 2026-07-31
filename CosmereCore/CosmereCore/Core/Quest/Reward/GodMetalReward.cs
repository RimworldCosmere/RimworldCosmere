using System.Collections.Generic;
using Cosmere.Core.Def;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.Core.Quest.Reward;

/// <summary>
///     Drops a quantity of one metal. Used for every god-metal payout in the Scadrial arc.
///     The count range is inclusive on both ends.
/// </summary>
public class GodMetalReward : QuestReward {
    public int countMax = 1;
    public int countMin = 1;
    public MetalDef? metal;

    public override void Give(QuestBuildContext ctx) {
        if (metal?.Item == null || ctx.map == null) {
            Logger.Error($"GodMetalReward on {ctx.def?.defName} could not resolve a metal thing.");
            return;
        }

        int count = Rand.RangeInclusive(countMin, countMax);
        Verse.Thing stack = ThingMaker.MakeThing(metal.Item);
        stack.stackCount = count;

        DropPodUtility.DropThingsNear(
            DropCellFinder.TradeDropSpot(ctx.map),
            ctx.map,
            new List<Verse.Thing> { stack }
        );
    }

    public override string Describe() {
        if (metal == null) return string.Empty;
        return countMin == countMax
            ? "CC_Quest_Reward_Metal_Exact".Translate(metal.coloredLabel.Named("METAL"), countMin.Named("COUNT"))
                .Resolve()
            : "CC_Quest_Reward_Metal_Range".Translate(
                metal.coloredLabel.Named("METAL"),
                countMin.Named("MIN"),
                countMax.Named("MAX")
            ).Resolve();
    }

    public override string? ConfigError() {
        if (metal == null) return "GodMetalReward has no metal.";
        if (countMin < 1) return "GodMetalReward countMin must be at least 1.";
        if (countMax < countMin) return "GodMetalReward countMax is below countMin.";
        return null;
    }
}
