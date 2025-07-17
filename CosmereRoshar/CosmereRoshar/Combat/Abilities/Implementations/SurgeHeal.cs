using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar {

    /// Surge regrowth plants
    public class CompProperties_AbilitySurgeHeal : CompProperties_AbilityEffect {
        public float stormLightCost;

        public CompProperties_AbilitySurgeHeal() {
            this.compClass = typeof(CompAbilityEffect_SurgeHeal);
        }
    }
    public class CompAbilityEffect_SurgeHeal : CompAbilityEffect {
        public new CompProperties_AbilitySurgeHeal Props => this.props as CompProperties_AbilitySurgeHeal;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            // 1) Validate target
            if (!target.IsValid || target.Thing == null || !target.Thing.Spawned) {
                Log.Warning("[HealSurge] Invalid target.");
                return;
            }

            Pawn caster = this.parent.pawn as Pawn;
            if (caster == null) {
                return;
            }

            if (caster.GetComp<CompStormlight>() == null) {
                Log.Warning($"[heal] CompStormlight is null!");
                return;
            }


            // 3) heal target
            healFunction(target.Thing);
        }



        private void healFunction(Thing targetThing) {
            Map map = targetThing.Map;

            Pawn targetPawn = targetThing as Pawn;
            if (targetPawn == null) {
                return;
            }

            radiantHeal(targetPawn);
        }

        private void healMissingParts(Pawn pawn, Need_RadiantProgress radiantNeed, CompStormlight casterComp, Pawn caster) {
            if (radiantNeed != null && radiantNeed.IdealLevel >= 3) {
                var missingParts = pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().OrderByDescending(h => h.Severity).ToList();
                foreach (var injury in missingParts) {
                    float cost = 175f;  // More severe wounds cost more stormlight
                    if (casterComp.Stormlight < cost)
                        break;
                    pawn.health.hediffSet.hediffs.Remove(injury);
                    casterComp.drawStormlight(cost);
                    RadiantUtility.GiveRadiantXP(caster, 20f);
                }
            }
            else { Log.Message("Ideal level to low to heal missing part"); }
        }

        private void healInjuries(Pawn pawn, Need_RadiantProgress radiantNeed, CompStormlight casterComp, Pawn caster) {
            var injuries = pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().OrderByDescending(h => h.Severity).ToList();
            bool stillInjured = true;
            while (stillInjured && casterComp.HasStormlight) {
                stillInjured = false;
                foreach (var injury in injuries) {
                    float cost = 1f;
                    if (casterComp.Stormlight < cost)
                        break;

                    float healAmount = (0.008f) + ((float)radiantNeed.IdealLevel * 2f) / 10f;
                    injury.Heal(healAmount);
                    casterComp.drawStormlight(cost);
                    RadiantUtility.GiveRadiantXP(caster, 5f);
                    if (injury.Severity > 0) stillInjured = true;
                }
            }
        }
        private void radiantHeal(Pawn pawn) {
            Pawn caster = this.parent.pawn as Pawn;
            CompStormlight casterComp = caster.GetComp<CompStormlight>();
            if (casterComp != null) {
                Need_RadiantProgress radiantNeed = caster.needs.TryGetNeed<Need_RadiantProgress>();
                if (radiantNeed == null) {
                    Log.Error("[HealSurge] need was null");
                    return;
                }

                healMissingParts(pawn, radiantNeed, casterComp, caster);
                healInjuries(pawn, radiantNeed, casterComp, caster);
            }
        }
    }




}
