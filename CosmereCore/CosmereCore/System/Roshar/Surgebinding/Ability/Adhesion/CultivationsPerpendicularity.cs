using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Adhesion;

public class CultivationsPerpendicularity : OpenPerpendicularity {
    public CultivationsPerpendicularity(Pawn pawn) : base(pawn) { }

    public CultivationsPerpendicularity(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    protected override string AuraMoteDefName => "Cosmere_Roshar_Thing_CultivationsPerpendicularityAura";

    protected override void RefillRadiantStormlight(Pawn ally) {
        // cultivation's perpendicularity grows and heals, it does not hand back stormlight
    }
}
