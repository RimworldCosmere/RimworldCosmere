using RimWorld;
using Verse;

namespace Cosmere.Core.Ideology;

public class ThoughtWorker_Precept_NoFactionLeader : ThoughtWorker_Precept {
    protected override ThoughtState ShouldHaveThought(Pawn p) {
        if (p.Faction == null) return ThoughtState.Inactive;
        // Happy when there's no leader - living as equals
        return p.Faction.leader == null;
    }
}