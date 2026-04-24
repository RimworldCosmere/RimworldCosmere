using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Surgebinding.Utility;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Abrasion;

public class Grip : SurgebindingAbility {
    public Grip(Pawn pawn) : base(pawn) { }
    public Grip(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + gene.currentIdeal * 0.5f);
    }

    protected override void OnEnable() {
        base.OnEnable();
        SurgebindingHediffUtility.GetOrAddHediff(pawn, this, def.hediff);

        RimWorld.Ability slickAbility = pawn.abilities.GetAbility(
            DefDatabase<AbilityDef>.GetNamed("Cosmere_Roshar_Ability_Slick")
        );
        if (slickAbility is SurgebindingAbility { status.isActive: true } slick) {
            slick.UpdateStatus(Active.Off);
        }
    }
}