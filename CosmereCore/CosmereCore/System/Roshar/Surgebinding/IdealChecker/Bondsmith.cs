using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.IdealChecker;

public class Bondsmith(RadiantOrderDef def) : AbstractIdealChecker(def) {
    public override bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        if (HasIncompatibleTrait(pawn, nextLevel)) return false;

        if (nextLevel >= 1) {
            RadiantTracker tracker = Current.Game.GetComponent<RadiantTracker>();
            if (tracker != null && !tracker.CanProgressBondsmith()) return false;
        }

        float friendships = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_FriendshipsFormed);

        if (nextLevel == 1) {
            return friendships >= ApplyDifficulty(5);
        }

        float conflicts = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_ConflictsResolved);

        if (nextLevel == 2) {
            return friendships >= ApplyDifficulty(10) && conflicts >= ApplyDifficulty(2);
        }

        if (nextLevel == 3) {
            return friendships >= ApplyDifficulty(15) &&
                   conflicts >= ApplyDifficulty(5) &&
                   CountPositiveFactions(20) >= 3;
        }

        if (nextLevel == 4) {
            float stabilityDays = pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_ColonyStabilityDays);
            return friendships >= ApplyDifficulty(25) &&
                   conflicts >= ApplyDifficulty(10) &&
                   stabilityDays >= ApplyDifficulty(30) &&
                   CountPositiveFactions(40) >= 5;
        }

        return false;
    }

    private static int CountPositiveFactions(int minGoodwill) {
        List<Faction> factions = Find.FactionManager.AllFactionsListForReading;
        int count = 0;
        for (int i = 0; i < factions.Count; i++) {
            Faction faction = factions[i];
            if (faction.IsPlayer) continue;
            if (faction.PlayerGoodwill >= minGoodwill) count++;
        }

        return count;
    }

    public override string? GetRequirementsText(int idealIndex) {
        return idealIndex switch {
            0 => null,
            1 => $"{ApplyDifficulty(5):0}+ friendships (max 3 Bondsmiths) | Skill 4+",
            2 => $"{ApplyDifficulty(10):0}+ friendships, {ApplyDifficulty(2):0}+ conflicts resolved | Skill 8+",
            3 =>
                $"{ApplyDifficulty(15):0}+ friendships, {ApplyDifficulty(5):0}+ conflicts, 3+ allied factions | Skill 14+",
            4 =>
                $"{ApplyDifficulty(25):0}+ friendships, {ApplyDifficulty(10):0}+ conflicts, {ApplyDifficulty(30):0}+ stability days, 5+ allied factions | Skill 18+",
            _ => null,
        };
    }

    public override bool Satisfy(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        return base.Satisfy(pawn, surgebinder, nextLevel);
    }
}