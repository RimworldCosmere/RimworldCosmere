using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.IdealChecker;

public class Skybreaker(RadiantOrderDef def) : AbstractIdealChecker(def) {
    public override bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        if (HasIncompatibleTrait(pawn, nextLevel)) return false;

        float arrests = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_ArrestsMade);
        float complianceDays = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_ZoneComplianceDays);

        if (nextLevel == 1) {
            return arrests >= 1;
        }

        if (nextLevel == 2) {
            return arrests >= ApplyDifficulty(5) && complianceDays >= ApplyDifficulty(30);
        }

        if (nextLevel == 3) {
            float judgments = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_JudgmentsPassed);
            return arrests >= ApplyDifficulty(10) &&
                   complianceDays >= ApplyDifficulty(60) &&
                   judgments >= ApplyDifficulty(3);
        }

        if (nextLevel == 4) {
            return arrests >= ApplyDifficulty(15) && complianceDays >= ApplyDifficulty(90);
        }

        return false;
    }

    public override string? GetRequirementsText(int idealIndex) {
        return idealIndex switch {
            0 => null,
            1 => "Make 1+ arrest | Skill 4+",
            2 => $"{ApplyDifficulty(5):0}+ arrests, {ApplyDifficulty(30):0}+ zone compliance days | Skill 8+",
            3 =>
                $"{ApplyDifficulty(10):0}+ arrests, {ApplyDifficulty(60):0}+ compliance days, {ApplyDifficulty(3):0}+ judgments | Skill 14+",
            4 => $"{ApplyDifficulty(15):0}+ arrests, {ApplyDifficulty(90):0}+ compliance days | Skill 18+",
            _ => null,
        };
    }

    public override bool ConsummateOath(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        return base.ConsummateOath(pawn, surgebinder, nextLevel);
    }
}
