using CosmereScadrial.Abilities.Allomancy;
using RimWorld;
using Verse;

namespace CosmereScadrial.Command {
    public class AllomancyCommand(AbstractAbility ability, Pawn pawn) : Command_Ability(ability, pawn) {
        public override float GetWidth(float maxWidth) {
            return 75f;
        }
    }
}