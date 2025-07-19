using CosmereRoshar.Comp.Thing;
using RimWorld;
using Verse;

namespace CosmereRoshar.Combat.Abilities.Implementations;

public class ToggleStormlightProperties : CompProperties_AbilityEffect {
    public ToggleStormlightProperties() {
        compClass = typeof(ToggleStormlight);
    }
}

public class ToggleStormlight : CompAbilityEffect {
    public new ToggleStormlightProperties props =>
        ((AbilityComp)this).props as ToggleStormlightProperties;

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