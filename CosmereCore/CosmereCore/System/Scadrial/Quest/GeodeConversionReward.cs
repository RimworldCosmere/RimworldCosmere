using Cosmere.Core.Quest;
using Cosmere.Core.Quest.Reward;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Quest;

/// <summary>
///     Consumes every geode the colony has and converts them to atium. Rolls per geode, but
///     guarantees one bead per full twenty so a run that hauled the quota is never empty-handed.
/// </summary>
public class GeodeConversionReward : QuestReward {
    public float chancePerGeode = 0.08f;
    public ThingDef? geodeDef;
    public int guaranteedPer = 20;
    public ThingDef? metalDef;

    public override void Give(QuestBuildContext ctx) {
        ThingDef? geode = geodeDef;
        ThingDef? metal = metalDef;
        if (geode == null || metal == null) return;

        int consumed = HomeStock.ConsumeAll(geode);
        if (consumed <= 0) return;

        int rolled = 0;
        for (int i = 0; i < consumed; i++) {
            if (Rand.Chance(chancePerGeode)) rolled++;
        }

        int floor = guaranteedPer > 0 ? consumed / guaranteedPer : 0;
        int total = rolled > floor ? rolled : floor;
        if (total <= 0) return;

        Verse.Thing beads = ThingMaker.MakeThing(metal);
        beads.stackCount = total;
        GenPlace.TryPlaceThing(beads, ctx.map.Center, ctx.map, ThingPlaceMode.Near);

        Messages.Message(
            "CS_Quest_Hathsin_GeodesCracked".Translate(consumed.Named("GEODES"), total.Named("BEADS")),
            beads,
            MessageTypeDefOf.PositiveEvent
        );
    }

    public override string Describe() {
        return "CS_Quest_Reward_GeodeConversion".Translate().Resolve();
    }

    public override string? ConfigError() {
        if (geodeDef == null) return "GeodeConversionReward has no geodeDef.";
        if (metalDef == null) return "GeodeConversionReward has no metalDef.";
        if (chancePerGeode < 0f || chancePerGeode > 1f) {
            return "GeodeConversionReward chancePerGeode must be between 0 and 1.";
        }

        return null;
    }
}
