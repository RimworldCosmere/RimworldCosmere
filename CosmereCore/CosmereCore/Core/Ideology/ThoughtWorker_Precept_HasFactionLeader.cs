using RimWorld;
using Verse;

namespace Cosmere.Core.Ideology;

public class ThoughtWorker_Precept_HasFactionLeader : ThoughtWorker_Precept {
    protected override ThoughtState ShouldHaveThought(Pawn p) {
        if (p.Faction == null) return ThoughtState.Inactive;
        return p.Faction.leader != null;
    }
}
