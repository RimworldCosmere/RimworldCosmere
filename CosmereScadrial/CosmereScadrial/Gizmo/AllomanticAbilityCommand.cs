using CosmereScadrial.Ability.Allomancy;
using RimWorld;
using Verse;

namespace CosmereScadrial.Command;

public class AllomanticAbilityCommand(AbstractAbility ability, Pawn pawn) : Command_Ability(ability, pawn) {
    public override bool Visible => false;
}