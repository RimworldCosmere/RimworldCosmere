using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Adhesion;

public class SiblingBlessing : OpenPerpendicularity {
    public SiblingBlessing(Pawn pawn) : base(pawn) { }

    public SiblingBlessing(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    protected override string AuraMoteDefName => "Cosmere_Roshar_Thing_SiblingBlessingAura";

    protected override void RefillRadiantStormlight(Pawn ally) { }

    protected override void HealAlly(Pawn ally) { }
}
