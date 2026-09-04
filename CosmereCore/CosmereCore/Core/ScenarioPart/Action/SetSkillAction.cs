using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

public class SetSkillAction : ProgressionAction {
    public int level = -1;

    /// <summary>Added to the pawn's current level. Use instead of level to reward growth.</summary>
    public int levelOffset;

    public string? passion;

    /// <summary>Steps the passion up by this much, capped at Major.</summary>
    public int passionOffset;

    public string pawnName = string.Empty;
    public string skill = string.Empty;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            Log.Warn($"ScenarioProgression: Pawn '{pawnName}' not found for SetSkill");
            return;
        }

        SkillDef? skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(skill);
        if (skillDef == null) {
            Log.Warn($"ScenarioProgression: Skill '{skill}' not found");
            return;
        }

        SkillRecord? record = pawn.skills?.GetSkill(skillDef);
        if (record == null) return;

        if (level >= 0) record.Level = level;

        // SkillRecord.Level clamps to 0-20 on set, so an offset past the cap is safe.
        if (levelOffset != 0) record.Level += levelOffset;

        if (passion != null) {
            record.passion = (Passion)ParseHelper.FromString(passion, typeof(Passion));
        }

        if (passionOffset != 0) {
            int stepped = (int)record.passion + passionOffset;
            if (stepped < 0) stepped = 0;
            if (stepped > (int)Passion.Major) stepped = (int)Passion.Major;
            record.passion = (Passion)stepped;
        }
    }

    public override string? Describe() {
        SkillDef? skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(skill);
        if (skillDef == null) return null;

        if (levelOffset != 0) {
            return "CC_Progression_Effect_SkillGain".Translate(
                pawnName.Named("PAWN"),
                levelOffset.Named("COUNT"),
                skillDef.label.Named("SKILL")
            ).Resolve();
        }

        if (level >= 0) {
            return "CC_Progression_Effect_SkillSet".Translate(
                pawnName.Named("PAWN"),
                skillDef.label.Named("SKILL"),
                level.Named("COUNT")
            ).Resolve();
        }

        return null;
    }
}
