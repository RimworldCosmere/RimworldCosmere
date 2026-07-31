using Concord;
using Cosmere.Core.Def;
using Cosmere.Core.DefModExtension;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class GemSpawnAfterMinePatch : Mineable {
    private const float PlainRockGemChance = 0.01f;

    private static ThingDef randomGemDef =>
        DefDatabase<GemDef>.AllDefsListForReading
            .RandomElementByWeight(x => x.MineableItem.building.mineableScatterCommonality)
            .Item;

    [Inject(At.Return, "TrySpawnYield", parameterTypes: [typeof(Map), typeof(bool), typeof(Pawn)])]
    private void AfterTrySpawnYield(Map? map, bool moteOnWaste, Pawn? pawn) {
        if (map == null) return;
        if (def.HasModExtension<GemsLinked>()) return;
        if (!Rand.Chance(PlainRockGemChance)) return;

        GenPlace.TryPlaceThing(ThingMaker.MakeThing(randomGemDef), Position, map, ThingPlaceMode.Direct);
    }
}
