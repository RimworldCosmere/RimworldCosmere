using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Adhesion;

public class SiblingBlessing : OpenPerpendicularity {
    public SiblingBlessing(Pawn pawn) : base(pawn) { }

    public SiblingBlessing(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    protected override string AuraMoteDefName => "Cosmere_Roshar_Thing_SiblingBlessingAura";

    protected override void RefillRadiantStormlight(Pawn ally) {
        // the sibling's blessing only sustains the tower, it grants nothing to the pawn
    }

    protected override void HealAlly(Pawn ally) {
        // the sibling's blessing only sustains the tower, it grants nothing to the pawn
    }
}
