using Cosmere.Core.Comp.Game;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Thought;

public class NahelBondOpinionWorker : ThoughtWorker {
    protected override ThoughtState CurrentStateInternal(Pawn p) {
        return ThoughtState.Inactive;
    }

    protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other) {
        PawnRelationDef? nahelBondDef =
            DefDatabase<PawnRelationDef>.GetNamedSilentFail("Cosmere_Roshar_Relation_NahelBond");
        if (nahelBondDef == null) return false;

        if (!p.relations.DirectRelationExists(nahelBondDef, other)) return false;

        float connection = SpiritWeb.Instance?.GetConnectionValue(p, other) ?? 0f;

        if (connection >= 0.7f) return ThoughtState.ActiveAtStage(0);
        if (connection >= 0.4f) return ThoughtState.ActiveAtStage(1);
        if (connection >= 0.15f) return ThoughtState.ActiveAtStage(2);
        return ThoughtState.ActiveAtStage(3);
    }
}