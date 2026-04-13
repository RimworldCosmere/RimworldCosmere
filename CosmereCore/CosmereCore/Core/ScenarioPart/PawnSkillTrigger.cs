using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class PawnSkillTrigger : ProgressionTrigger {
    public string pawnName = "";
    public string skill = "";
    public int minLevel;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) return false;

        SkillDef? skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(skill);
        if (skillDef == null) return false;

        SkillRecord? record = pawn.skills?.GetSkill(skillDef);
        return record != null && record.Level >= minLevel;
    }
}
