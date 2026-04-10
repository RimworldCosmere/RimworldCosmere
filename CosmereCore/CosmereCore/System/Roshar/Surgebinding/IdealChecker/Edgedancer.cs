using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.IdealChecker;

public class Edgedancer(RadiantOrderDef def) : AbstractIdealChecker(def) {
    public override bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        if (HasIncompatibleTrait(pawn, nextLevel)) return false;

        float forgottenTended = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_ForgottenTended);
        float deadHonored = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_DeadHonored);

        if (nextLevel == 1) {
            return forgottenTended >= ApplyDifficulty(5);
        }

        if (nextLevel == 2) {
            return forgottenTended >= ApplyDifficulty(15) && deadHonored >= ApplyDifficulty(3);
        }

        if (nextLevel == 3) {
            float graveVisits = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_GraveVisits);
            return forgottenTended >= ApplyDifficulty(30) && deadHonored >= ApplyDifficulty(8) && graveVisits >= ApplyDifficulty(5);
        }

        if (nextLevel == 4) {
            return forgottenTended >= ApplyDifficulty(50) && deadHonored >= ApplyDifficulty(15);
        }

        return false;
    }

    public override string? GetRequirementsText(int idealIndex) {
        return idealIndex switch {
            0 => null,
            1 => $"Tend {ApplyDifficulty(5):0}+ forgotten | Skill 4+",
            2 => $"Tend {ApplyDifficulty(15):0}+ forgotten, honor {ApplyDifficulty(3):0}+ dead | Skill 8+",
            3 => $"Tend {ApplyDifficulty(30):0}+ forgotten, honor {ApplyDifficulty(8):0}+ dead, {ApplyDifficulty(5):0}+ grave visits | Skill 14+",
            4 => $"Tend {ApplyDifficulty(50):0}+ forgotten, honor {ApplyDifficulty(15):0}+ dead | Skill 18+",
            _ => null,
        };
    }

    public override bool Satisfy(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        return base.Satisfy(pawn, surgebinder, nextLevel);
    }
}
