using Cosmere.Scadrial.Allomancy.Ability;
using RimWorld;
using Verse;

namespace Cosmere.Scadrial.Gizmo;

public class AllomanticAbilityCommand(AbstractAllomancyAbility ability, Pawn pawn) : Command_Ability(ability, pawn) {
    public override bool Visible => false;
}