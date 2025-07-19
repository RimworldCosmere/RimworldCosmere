using CosmereRoshar.Comp.Thing;
using RimWorld;
using Verse;

namespace CosmereRoshar.Combat.Abilities.Implementations;

public class SurgeAbrasionProperties : CompProperties_AbilityEffect {
    public SurgeAbrasionProperties() {
        compClass = typeof(SurgeAbrasion);
    }
}

public class SurgeAbrasion : CompAbilityEffect {
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