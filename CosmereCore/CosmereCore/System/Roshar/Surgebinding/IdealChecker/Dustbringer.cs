using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.IdealChecker;

public class Dustbringer(RadiantOrderDef def) : AbstractIdealChecker(def) {
    public override bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        if (HasIncompatibleTrait(pawn, nextLevel)) return false;

        float kills = pawn.records.GetValue(RimWorld.RecordDefOf.KillsHumanlikes)
                      + pawn.records.GetValue(RimWorld.RecordDefOf.KillsMechanoids);
        float furyMastered = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_FuryMastered);

        if (nextLevel == 1) {
            return kills >= ApplyDifficulty(10);
        }

        if (nextLevel == 2) {
            return kills >= ApplyDifficulty(25) && furyMastered >= ApplyDifficulty(3);
        }

        if (nextLevel == 3) {
            float cleanRaids = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_CleanRaids);
            return kills >= ApplyDifficulty(50) && furyMastered >= ApplyDifficulty(10) && cleanRaids >= 1;
        }

        if (nextLevel == 4) {
            return kills >= ApplyDifficulty(100) && furyMastered >= ApplyDifficulty(20);
        }

        return false;
    }

    public override string? GetRequirementsText(int idealIndex) {
        return idealIndex switch {
            0 => null,
            1 => $"{ApplyDifficulty(10):0}+ kills | Skill 4+",
            2 => $"{ApplyDifficulty(25):0}+ kills, {ApplyDifficulty(3):0}+ fury mastered | Skill 8+",
            3 => $"{ApplyDifficulty(50):0}+ kills, {ApplyDifficulty(10):0}+ fury mastered, 1+ clean raid | Skill 14+",
            4 => $"{ApplyDifficulty(100):0}+ kills, {ApplyDifficulty(20):0}+ fury mastered | Skill 18+",
            _ => null,
        };
    }

    public override bool Satisfy(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        return base.Satisfy(pawn, surgebinder, nextLevel);
    }
}
