using CosmereRoshar.Comps.WeaponsAndArmor;
using RimWorld;
using Verse;

namespace CosmereRoshar.Combat.Abilities.Implementations;

/// SUMMON BLADE ABILITY
public class CompPropertiesAbilitySpawnEquipment : CompProperties_AbilityEffect {
    public ThingDef thingDef; // The weapon to spawn


    public CompPropertiesAbilitySpawnEquipment() {
        compClass = typeof(CompAbilityEffectSpawnEquipment);
    }
}

public class CompAbilityEffectSpawnEquipment : CompAbilityEffect {
    public ThingWithComps bladeObject;
    public new CompPropertiesAbilitySpawnEquipment props => (CompPropertiesAbilitySpawnEquipment)((AbilityComp)this).props;


    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Deep.Look(ref bladeObject, "bladeObject", null);
    }

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        if (target == null || target.Cell == null) {
            Log.Warning("[CosmereRoshar] SpawnEquipment target is null, defaulting to caster.");
            target = new LocalTargetInfo(parent.pawn); // Default to the caster
        }

        if (props.thingDef == null) {
            Log.Error("[CosmereRoshar] SpawnEquipment failed: thingDef not set.");
            return;
        }

        Pawn pawn = parent.pawn;
        CheckAndDropWeapon(ref pawn);
        ToggleBlade(ref pawn);
    }

    private void CheckAndDropWeapon(ref Pawn pawn) {
        if (pawn == null) return;
        if (pawn.equipment.Primary != null) {
            if (pawn.equipment.Primary.def.defName.Equals(props.thingDef.ToString())) return;

            // Drop the existing weapon
            ThingWithComps droppedWeapon;
            pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out droppedWeapon, pawn.Position, false);
        }
    }

    private void ToggleBlade(ref Pawn pawn) {
        CompAbilityEffectSpawnEquipment abilityComp =
            pawn.GetAbilityComp<CompAbilityEffectSpawnEquipment>(CosmereRosharDefs.WhtwlSummonShardblade.defName);
        if (abilityComp == null) return;
        if (abilityComp.bladeObject == null) {
            Log.Warning("[CosmereRoshar] toggleBlade: bladeObject is null, attempting recovery from equipment...");
            abilityComp.bladeObject = pawn.equipment?.AllEquipmentListForReading
                .FirstOrDefault(e => e.def.defName == CosmereRosharDefs.WhtwlMeleeWeaponShardblade.defName);

            if (abilityComp.bladeObject == null) {
                Log.Error("[CosmereRoshar] toggleBlade: Failed to recover bladeObject, aborting toggle.");
                return;
            }
        }

        CompShardblade blade = abilityComp.bladeObject.GetComp<CompShardblade>();
        if (blade == null) return;

        if (blade.IsBladeSpawned() == false) {
            Log.Message($"[CosmereRoshar] Radiant {pawn.Name} summoned shard blade!");
            blade.Summon();
        } else {
            Log.Message($"[CosmereRoshar] Radiant {pawn.Name} dismissed the blade");
            blade.DismissBlade(pawn);
        }
    }
}

/// BREAK BOND ABILITY
public class CompPropertiesAbilityBreakBond : CompProperties_AbilityEffect {
    public ThingDef thingDef;

    public CompPropertiesAbilityBreakBond() {
        compClass = typeof(CompAbilityEffectBreakBondWithSword);
    }
}

public class CompAbilityEffectBreakBondWithSword : CompAbilityEffect {
    public new CompPropertiesAbilityBreakBond props => (CompPropertiesAbilityBreakBond)((AbilityComp)this).props;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        if (target == null || target.Cell == null) {
            Log.Warning("[CosmereRoshar] break bond target is null, defaulting to caster.");
            target = new LocalTargetInfo(parent.pawn); // Default to the caster
        }

        if (props.thingDef == null) {
            Log.Error("[CosmereRoshar] break bond failed: thingDef not set.");
            return;
        }

        Pawn pawn = parent.pawn;
        CheckAndDropWeapon(ref pawn);
    }

    private void CheckAndDropWeapon(ref Pawn pawn) {
        if (pawn.equipment.Primary != null) {
            Log.Message("[CosmereRoshar] try to break bond.");

            if (pawn.equipment.Primary.def.defName.Equals(props.thingDef.ToString())) {
                CompShardblade blade = pawn.equipment.Primary.GetComp<CompShardblade>();
                blade.SeverBond(pawn);
            } else {
                Log.Error(
                    $"[CosmereRoshar] eq name is '{pawn.equipment.Primary.def.defName}', thingDef is '{props.thingDef}'"
                );
            }
        }
    }
}