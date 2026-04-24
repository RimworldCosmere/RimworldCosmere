using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.IdealChecker;

public class Lightweaver(RadiantOrderDef def) : AbstractIdealChecker(def) {
    public override bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        if (HasIncompatibleTrait(pawn, nextLevel)) return false;

        if (nextLevel == 1) {
            return surgebinder.MentalBreaksSurvived >= 1 || surgebinder.ArtPiecesCreated >= ApplyDifficulty(3);
        }

        if (nextLevel == 2) {
            return surgebinder.LostCloseRelationship || surgebinder.ArtPiecesGood >= ApplyDifficulty(2);
        }

        if (nextLevel == 3) {
            return surgebinder.RecoveredFromMajorHediff || surgebinder.ArtPiecesExcellent >= 1;
        }

        if (nextLevel == 4) {
            return surgebinder.TraumaEventCount >= 3 ||
                   surgebinder.ArtPiecesLegendary >= 1 ||
                   surgebinder.TraumaEventCount >= 2 && surgebinder.ArtPiecesCreated >= ApplyDifficulty(5);
        }

        return false;
    }

    public override string? GetRequirementsText(int idealIndex) {
        return idealIndex switch {
            0 => null,
            1 => $"Survive 1 mental break OR create {ApplyDifficulty(3):0}+ art | Skill 4+",
            2 => $"Lose close relationship OR {ApplyDifficulty(2):0}+ good art | Skill 8+",
            3 => "Recover from major hediff OR 1+ excellent art | Skill 14+",
            4 => $"3+ trauma events OR 1 legendary art OR (2+ trauma, {ApplyDifficulty(5):0}+ art) | Skill 18+",
            _ => null,
        };
    }
}