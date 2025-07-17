using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace CosmereRoshar {

    public class CompProperties_AbilitySurgeDivision : CompProperties_AbilityEffect {
        public float stormLightCost;
        public CompProperties_AbilitySurgeDivision() {
            this.compClass = typeof(CompAbilityEffect_SurgeDivision);
        }
    }
    public class CompAbilityEffect_SurgeDivision : CompAbilityEffect {
        public new CompProperties_AbilitySurgeDivision Props => this.props as CompProperties_AbilitySurgeDivision;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            // 1) Validate target
            if (!target.IsValid || target.Thing == null || !target.Thing.Spawned) {
                Log.Warning("[Division surge] Invalid target.");
                return;
            }

            Pawn caster = this.parent.pawn as Pawn;
            if (caster == null) {
                Log.Error($"[Division surge] caster is null!");
                return;
            }
            if (caster.GetComp<CompStormlight>() == null) {
                Log.Error($"[Division surge] no stormlight comp!");
                return;
            }

            tryToUseDivision(target.Thing as Pawn, caster);
        }

        private void tryToUseDivision(Pawn victim, Pawn caster) {
            if (victim == null || caster.GetComp<CompStormlight>().Stormlight < Props.stormLightCost) return;
            List<Thing> thingsIgnoredByExplosion = new List<Thing>() { caster };
            if (StormlightUtilities.RollTheDice(1, 25, 2) && caster.GetComp<CompStormlight>().Stormlight >= Props.stormLightCost * 1.5f) {
                //GenExplosion.DoExplosion(victim.Position, caster.Map, 1.0f, DamageDefOf.Vaporize, caster, -1, -1f, null, null, null, null, null, 0f, 1, null, applyDamageToExplosionCellsNeighbors: false, null, 0f, 1, 0f, damageFalloff: false, null, thingsIgnoredByExplosion, null);
                GenExplosion.DoExplosion(center: victim.Position,
                                         map: caster.Map,
                                         radius: 1.0f,
                                         damType: DamageDefOf.Vaporize,
                                         instigator: caster,
                                         damAmount: -1,
                                         armorPenetration: -1f,
                                         explosionSound: null,
                                         weapon: null,
                                         projectile: null,
                                         intendedTarget: null,
                                         postExplosionSpawnThingDef: null,
                                         postExplosionSpawnChance: 0f,
                                         postExplosionSpawnThingCount: 1,
                                         postExplosionGasType: null,    // new -1-
                                         postExplosionGasRadiusOverride: null,   // new -2-
                                         postExplosionGasAmount: 255,    // new -3-  (default)
                                         applyDamageToExplosionCellsNeighbors: false,
                                         preExplosionSpawnThingDef: null,
                                         preExplosionSpawnChance: 0f,
                                         preExplosionSpawnThingCount: 1,
                                         chanceToStartFire: 0f,
                                         damageFalloff: false,
                                         direction: null,
                                         ignoredThings: thingsIgnoredByExplosion
                                     );
                caster.GetComp<CompStormlight>().drawStormlight(Props.stormLightCost * 1.5f);
            }
            else {
                victim.TryAttachFire(Rand.Range(0.95f, 0.99f), caster);
                RadiantUtility.GiveRadiantXP(caster, 50f);
                caster.GetComp<CompStormlight>().drawStormlight(Props.stormLightCost);
            }
        }
    }


}
