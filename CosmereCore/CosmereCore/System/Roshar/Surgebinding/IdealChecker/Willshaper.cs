using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.IdealChecker;

public class Willshaper(RadiantOrderDef def) : AbstractIdealChecker(def) {
    public override bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        if (HasIncompatibleTrait(pawn, nextLevel)) return false;

        float prisonersFreed = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_PrisonersFreed);

        if (nextLevel == 1) {
            return prisonersFreed >= ApplyDifficulty(2);
        }

        float caravanTrips = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_CaravanTrips);

        if (nextLevel == 2) {
            return prisonersFreed >= ApplyDifficulty(5) && caravanTrips >= 1;
        }

        float structures = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_StructuresBuilt);

        if (nextLevel == 3) {
            return prisonersFreed >= ApplyDifficulty(10) &&
                   caravanTrips >= ApplyDifficulty(3) &&
                   structures >= ApplyDifficulty(30);
        }

        if (nextLevel == 4) {
            return prisonersFreed >= ApplyDifficulty(15) &&
                   caravanTrips >= ApplyDifficulty(5) &&
                   structures >= ApplyDifficulty(60);
        }

        return false;
    }

    public override string? GetRequirementsText(int idealIndex) {
        return idealIndex switch {
            0 => null,
            1 => $"Free {ApplyDifficulty(2):0}+ prisoners | Skill 4+",
            2 => $"Free {ApplyDifficulty(5):0}+ prisoners, 1+ caravan trip | Skill 8+",
            3 =>
                $"Free {ApplyDifficulty(10):0}+ prisoners, {ApplyDifficulty(3):0}+ caravan trips, {ApplyDifficulty(30):0}+ structures | Skill 14+",
            4 =>
                $"Free {ApplyDifficulty(15):0}+ prisoners, {ApplyDifficulty(5):0}+ caravan trips, {ApplyDifficulty(60):0}+ structures | Skill 18+",
            _ => null,
        };
    }

    public override bool ConsummateOath(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        return base.ConsummateOath(pawn, surgebinder, nextLevel);
    }
}