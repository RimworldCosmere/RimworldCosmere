using Cosmere.System.Roshar.Surgebinding.Utility;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Progression;

public class Regrowth : SurgebindingAbility {
    public Regrowth(Pawn pawn) : base(pawn) { }
    public Regrowth(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        float cost = def.beuPerTick / (1 << gene.currentIdeal);
        if (!gene.CanLowerReserve(cost)) return false;

        Pawn? targetPawn = target.Pawn;
        if (targetPawn == null || targetPawn.Dead) return false;

        HediffDef? hediffDef = def.hediff;
        if (hediffDef == null) return false;

        gene.RemoveFromReserve(cost);
        SurgebindingHediffUtility.GetOrAddHediff(targetPawn, this, hediffDef);

        return true;
    }
}