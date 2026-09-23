using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Surgebinding.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Tension;

public class RigidizeClothing : SurgebindingAbility {
    public RigidizeClothing(Pawn pawn) : base(pawn) { }

    public RigidizeClothing(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + Gene.CurrentIdeal * 0.5f);
    }

    protected override void OnEnable() {
        base.OnEnable();
        SurgebindingHediffUtility.GetOrAddHediff(pawn, this, def.hediff);
    }
}
