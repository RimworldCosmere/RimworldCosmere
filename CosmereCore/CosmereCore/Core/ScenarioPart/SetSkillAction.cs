using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class SetSkillAction : ProgressionAction {
    public string pawnName = "";
    public string skill = "";
    public int level = -1;
    public string? passion;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for SetSkill");
            return;
        }

        SkillDef? skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(skill);
        if (skillDef == null) {
            Logger.Warning($"ScenarioProgression: Skill '{skill}' not found");
            return;
        }

        SkillRecord? record = pawn.skills?.GetSkill(skillDef);
        if (record == null) return;

        if (level >= 0) record.Level = level;
        if (passion != null) {
            record.passion = (Passion)ParseHelper.FromString(passion, typeof(Passion));
        }
    }
}
