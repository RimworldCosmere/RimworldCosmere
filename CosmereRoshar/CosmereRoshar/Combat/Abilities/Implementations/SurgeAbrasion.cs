using CosmereRoshar.Comp.Thing;
using RimWorld;
using Verse;

namespace CosmereRoshar.Combat.Abilities.Implementations;

public class CompPropertiesAbilitySurgeAbrasion : CompProperties_AbilityEffect {
    public CompPropertiesAbilitySurgeAbrasion() {
        compClass = typeof(CompAbilityEffectSurgeAbrasion);
    }
}

public class CompAbilityEffectSurgeAbrasion : CompAbilityEffect {
    public new CompPropertiesAbilitySurgeAbrasion props => ((AbilityComp)this).props as CompPropertiesAbilitySurgeAbrasion;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        // 1) Validate target
        if (!target.IsValid || target.Thing == null || !target.Thing.Spawned) {
            Log.Warning("[Abrasion surge] Invalid target.");
            return;
        }


        Pawn caster = parent.pawn;
        if (caster == null) {
            Log.Error("[Abrasion surge] caster is null!");
            return;
        }

        if (caster.GetComp<Stormlight>() == null) {
            Log.Error("[Abrasion surge] no stormlight comp!");
            return;
        }

        caster.GetComp<Stormlight>().ToggleAbrasion();
    }
}