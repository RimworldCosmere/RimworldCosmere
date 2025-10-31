using Cosmere;
using Cosmere.System.Scadrial.Allomancy.Ability;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Gizmo;

public class AllomanticAbilityCommand(AllomancyAbility ability, Pawn pawn) : Command_Ability(ability, pawn) {
    public override bool Visible => false;
}