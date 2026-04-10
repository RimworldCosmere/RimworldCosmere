using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.GameCondition;

public class SuppressionField : RimWorld.GameCondition {
    private const int DrainTickInterval = 60;
    private const float BasePassiveDrainPerInterval = 0.3f;

    public float costMultiplier = 3f;

    public override void GameConditionTick() {
        base.GameConditionTick();
        if (!GenTicks.IsTickInterval(DrainTickInterval)) return;

        Map? map = SingleMap;
        if (map == null) return;

        List<Pawn> colonists = map.mapPawns.FreeColonistsAndPrisoners;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn pawn = colonists[i];
            if (pawn.genes == null) continue;

            Surgebinder? surgebinder = pawn.genes.GetFirstGeneOfType<Surgebinder>();
            if (surgebinder == null || !surgebinder.Active) continue;

            float drain = BasePassiveDrainPerInterval * costMultiplier;
            surgebinder.RemoveFromReserve(drain);
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref costMultiplier, "costMultiplier", 3f);
    }

    public static float GetCostMultiplier(Map map) {
        List<RimWorld.GameCondition> conditions = map.gameConditionManager.ActiveConditions;
        for (int i = 0; i < conditions.Count; i++) {
            if (conditions[i] is SuppressionField field) return field.costMultiplier;
        }
        return 1f;
    }

    public static bool IsActive(Map map) {
        List<RimWorld.GameCondition> conditions = map.gameConditionManager.ActiveConditions;
        for (int i = 0; i < conditions.Count; i++) {
            if (conditions[i] is SuppressionField) return true;
        }
        return false;
    }
}
