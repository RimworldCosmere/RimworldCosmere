using System.Collections.Generic;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.IdealChecker;

public class Elsecaller(RadiantOrderDef def) : AbstractIdealChecker(def) {
    public override bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        if (HasIncompatibleTrait(pawn, nextLevel)) return false;

        float research = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_ResearchCompleted);
        float challenges = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_ChallengesSurvived);

        if (nextLevel == 1) {
            return research >= 1 && CountSkillsAbove(pawn, 5) >= 2;
        }

        if (nextLevel == 2) {
            return research >= ApplyDifficulty(3) && CountSkillsAbove(pawn, 8) >= 3 && challenges >= 1;
        }

        if (nextLevel == 3) {
            return research >= ApplyDifficulty(6) && CountSkillsAbove(pawn, 10) >= 4 && challenges >= 2 && GetMaxSkillLevel(pawn) >= 15;
        }

        if (nextLevel == 4) {
            return research >= ApplyDifficulty(10) && CountSkillsAbove(pawn, 12) >= 5 && challenges >= ApplyDifficulty(3) && GetMaxSkillLevel(pawn) >= 18;
        }

        return false;
    }

    private static int CountSkillsAbove(Pawn pawn, int minLevel) {
        List<SkillRecord> skills = pawn.skills.skills;
        int count = 0;
        for (int i = 0; i < skills.Count; i++) {
            if (skills[i].TotallyDisabled) continue;
            if (skills[i].Level >= minLevel) count++;
        }
        return count;
    }

    private static int GetMaxSkillLevel(Pawn pawn) {
        List<SkillRecord> skills = pawn.skills.skills;
        int max = 0;
        for (int i = 0; i < skills.Count; i++) {
            if (skills[i].TotallyDisabled) continue;
            if (skills[i].Level > max) max = skills[i].Level;
        }
        return max;
    }

    public override string? GetRequirementsText(int idealIndex) {
        return idealIndex switch {
            0 => null,
            1 => "1+ research, 2+ skills at 5+ | Skill 4+",
            2 => $"{ApplyDifficulty(3):0}+ research, 3+ skills at 8+, 1+ challenge | Skill 8+",
            3 => $"{ApplyDifficulty(6):0}+ research, 4+ skills at 10+, 2+ challenges, max skill 15+ | Skill 14+",
            4 => $"{ApplyDifficulty(10):0}+ research, 5+ skills at 12+, {ApplyDifficulty(3):0}+ challenges, max skill 18+ | Skill 18+",
            _ => null,
        };
    }

    public override bool Satisfy(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        return base.Satisfy(pawn, surgebinder, nextLevel);
    }
}
