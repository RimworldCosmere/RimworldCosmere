using RimWorld;
using Verse;

namespace Cosmere.Roshar.Combat.Abilities.Implementations;

/// BREAK BOND ABILITY
public class BreakBondProperties : CompProperties_AbilityEffect {
    public ThingDef? thingDef;

    public BreakBondProperties() {
        compClass = typeof(BreakBondWithSword);
    }
}

public class BreakBondWithSword : CompAbilityEffect {
    public new BreakBondProperties props => (BreakBondProperties)base.props;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        if (target == null) {
            Log.Warning("[Roshar] break bond target is null, defaulting to caster.");
            target = new LocalTargetInfo(parent.pawn); // Default to the caster
        }

        if (props.thingDef == null) {
            Log.Error("[Roshar] break bond failed: thingDef not set.");
            return;
        }

        Pawn pawn = parent.pawn;
        CheckAndDropWeapon(ref pawn);
    }

    private void CheckAndDropWeapon(ref Pawn pawn) {
        if (pawn.equipment.Primary == null || props.thingDef == null) return;
        Log.Message("[Roshar] try to break bond.");

        if (!pawn.equipment.Primary.def.defName.Equals(props.thingDef.ToString())) {
            Log.Error(
                $"[Roshar] eq name is '{pawn.equipment.Primary.def.defName}', thingDef is '{props.thingDef}'"
            );
            return;
        }

        pawn.equipment.Primary.TryGetComp<ShardBlade>()?.SeverBond(pawn);
    }
}