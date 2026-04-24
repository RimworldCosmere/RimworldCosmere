using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.IdealChecker;

public class Truthwatcher(RadiantOrderDef def) : AbstractIdealChecker(def) {
    public override bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        if (HasIncompatibleTrait(pawn, nextLevel)) return false;

        float patientsSaved = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_PatientsSaved);

        if (nextLevel == 1) {
            return patientsSaved >= ApplyDifficulty(5);
        }

        float socialHealing = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_SocialHealing);

        if (nextLevel == 2) {
            return patientsSaved >= ApplyDifficulty(20) && socialHealing >= ApplyDifficulty(3);
        }

        float livesSaved = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_LivesSaved);

        if (nextLevel == 3) {
            return patientsSaved >= ApplyDifficulty(40) && socialHealing >= ApplyDifficulty(10) && livesSaved >= 1;
        }

        if (nextLevel == 4) {
            return patientsSaved >= ApplyDifficulty(60) &&
                   socialHealing >= ApplyDifficulty(20) &&
                   livesSaved >= ApplyDifficulty(5);
        }

        return false;
    }

    public override string? GetRequirementsText(int idealIndex) {
        return idealIndex switch {
            0 => null,
            1 => $"Save {ApplyDifficulty(5):0}+ patients | Skill 4+",
            2 => $"Save {ApplyDifficulty(20):0}+ patients, {ApplyDifficulty(3):0}+ social healing | Skill 8+",
            3 =>
                $"Save {ApplyDifficulty(40):0}+ patients, {ApplyDifficulty(10):0}+ social healing, 1+ life saved | Skill 14+",
            4 =>
                $"Save {ApplyDifficulty(60):0}+ patients, {ApplyDifficulty(20):0}+ social healing, {ApplyDifficulty(5):0}+ lives saved | Skill 18+",
            _ => null,
        };
    }

    public override bool Satisfy(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        return base.Satisfy(pawn, surgebinder, nextLevel);
    }
}