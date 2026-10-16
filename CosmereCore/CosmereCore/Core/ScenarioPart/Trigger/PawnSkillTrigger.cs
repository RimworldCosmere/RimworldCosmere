using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Trigger;

public class PawnSkillTrigger : IProgressionTrigger {
    public int minLevel;
    public string pawnName = string.Empty;
    public string skill = string.Empty;

    public bool IsMet(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = GameComponent_ScenarioProgression.FindPawnByName(pawnName);
        if (pawn == null) return false;

        SkillDef? skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(skill);
        if (skillDef == null) return false;

        SkillRecord? record = pawn.skills?.GetSkill(skillDef);
        return record != null && record.Level >= minLevel;
    }
}
