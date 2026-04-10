using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.IdealChecker;

public class Windrunner(RadiantOrderDef def) : AbstractIdealChecker(def) {
    public override bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        if (HasIncompatibleTrait(pawn, nextLevel)) return false;

        float rescues = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_PawnsRescued);

        if (nextLevel == 1) {
            return rescues >= 1;
        }

        if (nextLevel == 2) {
            float hostileRescues = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_HostilePawnsRescued);
            return hostileRescues >= 1 && rescues >= ApplyDifficulty(8);
        }

        if (nextLevel == 3) {
            return surgebinder.GriefRecoveryCount >= 2 && rescues >= ApplyDifficulty(15);
        }

        if (nextLevel == 4) {
            return surgebinder.WasDownedInCombat && rescues >= ApplyDifficulty(25);
        }

        return false;
    }

    public override string? GetRequirementsText(int idealIndex) {
        return idealIndex switch {
            0 => null,
            1 => "Rescue 1+ downed pawn | Skill 4+",
            2 => $"Rescue hostile/rival pawn, {ApplyDifficulty(8):0}+ total rescues | Skill 8+",
            3 => $"Survive grief 2x, {ApplyDifficulty(15):0}+ rescues | Skill 14+",
            4 => $"Survive being downed, {ApplyDifficulty(25):0}+ rescues | Skill 18+",
            _ => null,
        };
    }

    public override bool Satisfy(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        return base.Satisfy(pawn, surgebinder, nextLevel);
    }
}
