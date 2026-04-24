using Cosmere.System.Roshar.GameCondition;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Job.ThinkNode;

public class ConditionalHighstormActive : ThinkNode_Conditional {
    protected override bool Satisfied(Pawn pawn) {
        if (pawn.Map == null) return false;

        List<RimWorld.GameCondition> conditions = pawn.Map.gameConditionManager.ActiveConditions;
        for (int i = 0; i < conditions.Count; i++) {
            if (conditions[i] is Highstorm hs && hs.IsDangerousPhase) {
                return true;
            }
        }

        return false;
    }
}