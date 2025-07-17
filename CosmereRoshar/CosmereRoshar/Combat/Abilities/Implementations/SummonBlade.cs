using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar {
    /// SUMMON BLADE ABILITY
    public class CompProperties_AbilitySpawnEquipment : CompProperties_AbilityEffect {
        public ThingDef thingDef; // The weapon to spawn


        public CompProperties_AbilitySpawnEquipment() {
            this.compClass = typeof(CompAbilityEffect_SpawnEquipment);
        }
    }

    public class CompAbilityEffect_SpawnEquipment : CompAbilityEffect {
        public new CompProperties_AbilitySpawnEquipment Props => (CompProperties_AbilitySpawnEquipment)this.props;

        public ThingWithComps bladeObject = null;


        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Deep.Look(ref bladeObject, "bladeObject", null);

        }
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            if (target == null || target.Cell == null) {
                Log.Warning("[CosmereRoshar] SpawnEquipment target is null, defaulting to caster.");
                target = new LocalTargetInfo(parent.pawn); // Default to the caster
            }

            if (Props.thingDef == null) {
                Log.Error("[CosmereRoshar] SpawnEquipment failed: thingDef not set.");
                return;
            }

            Pawn pawn = parent.pawn;
            checkAndDropWeapon(ref pawn);
            toggleBlade(ref pawn);
        }

        private void checkAndDropWeapon(ref Pawn pawn) {
            if (pawn == null) return;
            if (pawn.equipment.Primary != null) {

                if (pawn.equipment.Primary.def.defName.Equals(Props.thingDef.ToString())) return;

                // Drop the existing weapon
                ThingWithComps droppedWeapon;
                pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out droppedWeapon, pawn.Position, forbid: false);

            }
        }

        private void toggleBlade(ref Pawn pawn) {
            CompAbilityEffect_SpawnEquipment abilityComp = pawn.GetAbilityComp<CompAbilityEffect_SpawnEquipment>(CosmereRosharDefs.whtwl_SummonShardblade.defName);
            if (abilityComp == null) return;
            if (abilityComp.bladeObject == null) {
                Log.Warning("[CosmereRoshar] toggleBlade: bladeObject is null, attempting recovery from equipment...");
                abilityComp.bladeObject = pawn.equipment?.AllEquipmentListForReading
                    .FirstOrDefault(e => e.def.defName == CosmereRosharDefs.whtwl_MeleeWeapon_Shardblade.defName);

                if (abilityComp.bladeObject == null) {
                    Log.Error("[CosmereRoshar] toggleBlade: Failed to recover bladeObject, aborting toggle.");
                    return;
                }
            }
            CompShardblade blade = abilityComp.bladeObject.GetComp<CompShardblade>();
            if (blade == null) return;

            if (blade.isBladeSpawned() == false) {
                Log.Message($"[CosmereRoshar] Radiant {pawn.Name} summoned shard blade!");
                blade.summon();
            }
            else {
                Log.Message($"[CosmereRoshar] Radiant {pawn.Name} dismissed the blade");
                blade.dismissBlade(pawn);
            }
        }
    }



    /// BREAK BOND ABILITY
    public class CompProperties_AbilityBreakBond : CompProperties_AbilityEffect {
        public ThingDef thingDef;

        public CompProperties_AbilityBreakBond() {
            this.compClass = typeof(CompAbilityEffect_BreakBondWithSword);
        }
    }
    public class CompAbilityEffect_BreakBondWithSword : CompAbilityEffect {
        public new CompProperties_AbilityBreakBond Props => (CompProperties_AbilityBreakBond)this.props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            if (target == null || target.Cell == null) {
                Log.Warning("[CosmereRoshar] break bond target is null, defaulting to caster.");
                target = new LocalTargetInfo(parent.pawn); // Default to the caster
            }

            if (Props.thingDef == null) {
                Log.Error("[CosmereRoshar] break bond failed: thingDef not set.");
                return;
            }

            Pawn pawn = parent.pawn;
            checkAndDropWeapon(ref pawn);

        }

        private void checkAndDropWeapon(ref Pawn pawn) {
            if (pawn.equipment.Primary != null) {
                Log.Message("[CosmereRoshar] try to break bond.");

                if (pawn.equipment.Primary.def.defName.Equals(Props.thingDef.ToString())) {
                    CompShardblade blade = pawn.equipment.Primary.GetComp<CompShardblade>();
                    blade.severBond(pawn);
                }
                else {
                    Log.Error($"[CosmereRoshar] eq name is '{pawn.equipment.Primary.def.defName}', thingDef is '{Props.thingDef.ToString()}'");
                }
            }
        }


    }
}