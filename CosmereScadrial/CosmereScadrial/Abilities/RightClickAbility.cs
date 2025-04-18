using CosmereScadrial.Defs;
using Verse;

namespace CosmereScadrial.Abilities {
    public abstract class RightClickAbility(RightClickAbilityDef def) : IRightClickAbilityInterface {
        public RightClickAbilityDef def = def;

        public abstract bool CanUse(Pawn caster, LocalTargetInfo target);
        public abstract void Execute(Pawn caster, LocalTargetInfo target);
    }
}