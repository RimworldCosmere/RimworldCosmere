using CosmereRoshar.Comp;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Comps;
using RimWorld;
using Verse;

namespace CosmereRoshar.Combat.Abilities.Implementations;

public class CompPropertiesAbilityToggleStormlight : CompProperties_AbilityEffect {
    public CompPropertiesAbilityToggleStormlight() {
        compClass = typeof(CompAbilityEffectAbilityToggleStormlight);
    }
}

public class CompAbilityEffectAbilityToggleStormlight : CompAbilityEffect {
    public new CompPropertiesAbilityToggleStormlight props => ((AbilityComp)this).props as CompPropertiesAbilityToggleStormlight;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        // 1) Validate target
        if (!target.IsValid || target.Thing == null || !target.Thing.Spawned) {
            Log.Warning("[Toggle Stormlight] Invalid target.");
            return;
        }


        Pawn caster = parent.pawn;
        if (caster == null) {
            Log.Error("[Toggle Stormlight] caster is null!");
            return;
        }

        if (caster.GetComp<Stormlight>() == null) {
            Log.Error("[Toggle Stormlight] no stormlight comp!");
            return;
        }

        caster.GetComp<Stormlight>().ToggleBreathStormlight();
    }
}