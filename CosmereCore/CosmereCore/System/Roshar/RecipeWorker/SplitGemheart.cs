using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.RecipeWorker;

public class SplitGemheart : Verse.RecipeWorker {
    private static readonly List<ThingDef> rawGemDefs = [
        Core.ThingDefOf.RawAmethyst,
        Core.ThingDefOf.RawDiamond,
        Core.ThingDefOf.RawEmerald,
        Core.ThingDefOf.RawGarnet,
        Core.ThingDefOf.RawHeliodor,
        Core.ThingDefOf.RawRuby,
        Core.ThingDefOf.RawSapphire,
        Core.ThingDefOf.RawSmokestone,
        Core.ThingDefOf.RawTopaz,
        Core.ThingDefOf.RawZircon,
    ];

    public override void Notify_IterationCompleted(Pawn billDoer, List<Verse.Thing> ingredients) {
        ThingDef cutGemDef = DefDatabase<ThingDef>.GetNamed("CutGem");
        int gemCount = Rand.RangeInclusive(4, 6);
        QualityCategory quality = QualityUtility.GenerateQualityCreatedByPawn(billDoer, RimWorld.SkillDefOf.Crafting);
        Map map = billDoer.Map;

        for (int i = 0; i < gemCount; i++) {
            ThingDef stuff = rawGemDefs.RandomElement();
            Verse.Thing gem = ThingMaker.MakeThing(cutGemDef, stuff);
            gem.stackCount = 1;

            CompQuality compQuality = gem.TryGetComp<CompQuality>();
            compQuality?.SetQuality(quality, ArtGenerationContext.Colony);

            GenPlace.TryPlaceThing(gem, billDoer.Position, map, ThingPlaceMode.Near);
        }
    }
}
