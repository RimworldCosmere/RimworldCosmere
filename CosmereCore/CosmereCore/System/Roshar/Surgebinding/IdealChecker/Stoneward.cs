using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.IdealChecker;

public class Stoneward(RadiantOrderDef def) : AbstractIdealChecker(def) {
    public override bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        if (HasIncompatibleTrait(pawn, nextLevel)) return false;

        float damageTaken = pawn.records.GetValue(RimWorld.RecordDefOf.DamageTaken);
        float structures = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_StructuresBuilt);

        if (nextLevel == 1) {
            return damageTaken > 0 && structures >= ApplyDifficulty(5);
        }

        float raidsDefended = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_RaidsDefended);

        if (nextLevel == 2) {
            return damageTaken >= ApplyDifficulty(500) &&
                   structures >= ApplyDifficulty(20) &&
                   raidsDefended >= ApplyDifficulty(3);
        }

        float lastStanding = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_LastStanding);

        if (nextLevel == 3) {
            return damageTaken >= ApplyDifficulty(1500) &&
                   structures >= ApplyDifficulty(50) &&
                   raidsDefended >= ApplyDifficulty(8) &&
                   lastStanding >= 1;
        }

        if (nextLevel == 4) {
            return damageTaken >= ApplyDifficulty(3000) &&
                   structures >= ApplyDifficulty(100) &&
                   raidsDefended >= ApplyDifficulty(15) &&
                   lastStanding >= 2;
        }

        return false;
    }

    public override string? GetRequirementsText(int idealIndex) {
        return idealIndex switch {
            0 => null,
            1 => $"Take damage, build {ApplyDifficulty(5):0}+ structures | Skill 4+",
            2 =>
                $"{ApplyDifficulty(500):0}+ damage taken, {ApplyDifficulty(20):0}+ structures, {ApplyDifficulty(3):0}+ raids defended | Skill 8+",
            3 =>
                $"{ApplyDifficulty(1500):0}+ damage, {ApplyDifficulty(50):0}+ structures, {ApplyDifficulty(8):0}+ raids, 1+ last standing | Skill 14+",
            4 =>
                $"{ApplyDifficulty(3000):0}+ damage, {ApplyDifficulty(100):0}+ structures, {ApplyDifficulty(15):0}+ raids, 2+ last standing | Skill 18+",
            _ => null,
        };
    }

    public override bool ConsummateOath(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        return base.ConsummateOath(pawn, surgebinder, nextLevel);
    }
}