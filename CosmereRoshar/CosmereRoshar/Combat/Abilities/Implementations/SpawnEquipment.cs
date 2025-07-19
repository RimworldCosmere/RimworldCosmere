using CosmereRoshar.Extensions;
using RimWorld;
using Verse;

namespace CosmereRoshar.Combat.Abilities.Implementations;

/// SUMMON BLADE ABILITY
public class SpawnEquipmentProperties : CompProperties_AbilityEffect {
    public ThingDef? thingDef; // The weapon to spawn

    public SpawnEquipmentProperties() {
        compClass = typeof(SpawnEquipment);
    }
}

public class SpawnEquipment : CompAbilityEffect {
    public ThingWithComps? bladeObject;
    private new SpawnEquipmentProperties props => (SpawnEquipmentProperties)base.props;

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Deep.Look(ref bladeObject, "bladeObject", null);
    }

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        // @todo This doesnt do anything
        if (target == null) {
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
        if (pawn.equipment.Primary == null || props.thingDef == null) return;
        if (pawn.equipment.Primary.def.defName.Equals(props.thingDef.ToString())) return;

        // Drop the existing weapon
        pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out _, pawn.Position, false);
    }

    private void ToggleBlade(ref Pawn pawn) {
        SpawnEquipment abilityComp =
            pawn.GetAbilityComp<SpawnEquipment>(CosmereRosharDefs.Cosmere_Roshar_SummonShardblade.defName);
        if (abilityComp.bladeObject == null) {
            Log.Warning("[CosmereRoshar] toggleBlade: bladeObject is null, attempting recovery from equipment...");
            abilityComp.bladeObject = pawn.equipment?.AllEquipmentListForReading
                .FirstOrDefault(e => e.def.defName == CosmereRosharDefs.Cosmere_Roshar_MeleeWeaponShardblade.defName);

            if (abilityComp.bladeObject == null) {
                Log.Error("[CosmereRoshar] toggleBlade: Failed to recover bladeObject, aborting toggle.");
                return;
            }
        }

        if (!abilityComp.bladeObject.TryGetComp(out ShardBlade blade)) return;

        if (!blade.IsBladeSpawned()) {
            Log.Message($"[CosmereRoshar] Radiant {pawn.Name} summoned shard blade!");
            blade.Summon();
        } else {
            Log.Message($"[CosmereRoshar] Radiant {pawn.Name} dismissed the blade");
            blade.DismissBlade(pawn);
        }
    }
}