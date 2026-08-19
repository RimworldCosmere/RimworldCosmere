using System;
using Cosmere.Core.Def;
using Cosmere.Core.ShardConnection;
using Cosmere.Core.UI.Codex;
using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.Core.Comp.Thing;

public class PawnConnectionStats : ThingComp {
    private Dictionary<string, int> earnedByWorld = [];

    public int EarnedFor(CosmereWorldDef? world) {
        if (world == null) return 0;

        return earnedByWorld.TryGetValue(world.defName, out int strength) ? strength : 0;
    }

    public void Grant(CosmereWorldDef? world, int amount) {
        if (world == null || amount == 0) return;

        earnedByWorld[world.defName] = ConnectionMath.Clamp(EarnedFor(world) + amount);
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Collections.Look(ref earnedByWorld, "earnedWorldConnection", LookMode.Value, LookMode.Value);
        earnedByWorld ??= [];
    }

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats() {
        if (parent is not Pawn pawn) yield break;

        StatCategoryDef category = DefDatabase<StatCategoryDef>.GetNamed("Cosmere");
        List<WorldConnection> worlds = WorldConnectionUtility.ActiveFor(pawn);
        for (int i = 0; i < worlds.Count; i++) {
            WorldConnection entry = worlds[i];
            yield return new StatDrawEntry(
                category,
                "CC_PlanetConnection_InfoLabel".Translate(entry.World.LabelCap.Named("WORLD")).Resolve(),
                "CC_PlanetConnection_InfoValue".Translate(
                    entry.Strength.Named("LEVEL"),
                    ConnectionMath.Max.Named("MAX")
                ).Resolve(),
                "CC_PlanetConnection_InfoReport".Translate(
                    pawn.Named("PAWN"),
                    entry.World.LabelCap.Named("WORLD"),
                    entry.Strength.Named("LEVEL"),
                    ConnectionMath.Max.Named("MAX"),
                    entry.Ancestry.Named("ANCESTRY"),
                    entry.Residence.Named("RESIDENCE"),
                    entry.Investiture.Named("INVESTITURE"),
                    entry.Earned.Named("EARNED")
                ).Resolve(),
                2100 - i,
                hyperlinks: new[] { new Dialog_InfoCard.Hyperlink(entry.World) }
            );
        }

        List<ShardDef> shards = [..DefDatabase<ShardDef>.AllDefsListForReading];
        shards.Sort((left, right) => string.Compare(left.LabelCap, right.LabelCap, StringComparison.Ordinal));
        int priority = 2000;
        for (int i = 0; i < shards.Count; i++) {
            ShardDef shard = shards[i];
            if (!ShardUtility.IsEnabled(shard)) continue;

            ConnectionBreakdown breakdown = ConnectionUtility.BreakdownFor(pawn, shard);
            int worldTie = global::System.Math.Max(breakdown.Ancestry, breakdown.Residence);
            string tier = ConnectionPalette.LabelKeyFor(ConnectionMath.TierOf(breakdown.Total)).Translate().Resolve();
            yield return new StatDrawEntry(
                category,
                "CC_Connection_InfoLabel".Translate(shard.LabelCap.Named("SHARD")).Resolve(),
                "CC_Connection_InfoValue".Translate(
                    tier.Named("TIER"),
                    breakdown.Total.Named("VALUE"),
                    ConnectionMath.Max.Named("MAX")
                ).Resolve(),
                "CC_Connection_InfoReport".Translate(
                    pawn.Named("PAWN"),
                    shard.LabelCap.Named("SHARD"),
                    worldTie.Named("WORLDTIE"),
                    breakdown.Investiture.Named("INVESTITURE"),
                    breakdown.Earned.Named("EARNED"),
                    (-breakdown.Held).Named("HELD"),
                    breakdown.Harmony.Named("HARMONY"),
                    breakdown.Total.Named("TOTAL")
                ).Resolve(),
                priority--,
                hyperlinks: new[] { new Dialog_InfoCard.Hyperlink(shard) }
            );
        }
    }
}
