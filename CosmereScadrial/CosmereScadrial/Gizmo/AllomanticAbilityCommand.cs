using Cosmere.Scadrial.Allomancy.Ability;
using RimWorld;
using Verse;

namespace Cosmere.Scadrial.Command;

public class AllomanticAbilityCommand(AbstractAbility ability, Pawn pawn) : Command_Ability(ability, pawn) {
    public override bool Visible => false;
}