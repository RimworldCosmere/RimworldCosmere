using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Def;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Records.Worker;

public class TimeSpentUsingSurgeWorker : RecordWorker {
    private SurgeDef? surgeCache;

    private SurgeDef surge => surgeCache ??=
        DefDatabase<SurgeDef>.GetNamed(def.defName.Replace("Cosmere_Roshar_Record_TimeSpentUsing_", ""));

    public override bool ShouldMeasureTimeNow(Pawn? pawn) {
        if (pawn?.abilities == null) return false;

        List<AbilityDef> surgeAbilities = surge.abilities;
        for (int i = 0; i < surgeAbilities.Count; i++) {
            RimWorld.Ability? ability = pawn.abilities.GetAbility(surgeAbilities[i]);
            if (ability is AbstractAbility abstractAbility && abstractAbility.status.isActive) {
                return true;
            }
        }

        return false;
    }
}
