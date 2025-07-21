using Cosmere.Scadrial.Allomancy.Ability;
using RimWorld;
using Verse;

namespace Cosmere.Scadrial.Gizmo;

public class AllomanticAbilityCommand(AllomancyAbility ability, Pawn pawn) : Command_Ability(ability, pawn) {
    public override bool Visible => false;
}