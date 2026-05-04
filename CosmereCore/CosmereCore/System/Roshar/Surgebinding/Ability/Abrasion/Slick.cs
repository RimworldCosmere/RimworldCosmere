using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Surgebinding.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Abrasion;

public class Slick : SurgebindingAbility {
    public Slick(Pawn pawn) : base(pawn) { }
    public Slick(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + Gene.CurrentIdeal * 0.5f);
    }

    protected override void OnEnable() {
        base.OnEnable();
        SurgebindingHediffUtility.GetOrAddHediff(pawn, this, def.hediff);

        RimWorld.Ability gripAbility = pawn.abilities.GetAbility(
            DefDatabase<AbilityDef>.GetNamed("Cosmere_Roshar_Ability_Grip")
        );
        if (gripAbility is SurgebindingAbility { status.IsActive: true } grip) {
            grip.UpdateStatus(Active.Off);
        }
    }
}