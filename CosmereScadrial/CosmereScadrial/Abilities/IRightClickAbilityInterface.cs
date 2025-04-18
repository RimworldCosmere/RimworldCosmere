using Verse;

namespace CosmereScadrial.Abilities {
    public interface IRightClickAbilityInterface {
        bool CanUse(Pawn caster, LocalTargetInfo target);
        void Execute(Pawn caster, LocalTargetInfo target);
    }
}