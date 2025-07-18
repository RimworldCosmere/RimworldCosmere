using System.Collections.Generic;
using System.Linq;
using CosmereRoshar.Comp.Thing;
using RimWorld;
using Verse;

namespace CosmereRoshar;

/// Surge regrowth plants
public class CompProperties_AbilitySurgeHeal : CompProperties_AbilityEffect {
    public float stormLightCost;

    public CompProperties_AbilitySurgeHeal() {
        compClass = typeof(CompAbilityEffect_SurgeHeal);
    }
}

public class CompAbilityEffect_SurgeHeal : CompAbilityEffect {
    public new CompProperties_AbilitySurgeHeal Props => props as CompProperties_AbilitySurgeHeal;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        // 1) Validate target
        if (!target.IsValid || target.Thing == null || !target.Thing.Spawned) {
            Log.Warning("[HealSurge] Invalid target.");
            return;
        }

        Pawn caster = parent.pawn;
        if (caster == null) {
            return;
        }

        if (caster.GetComp<CompStormlight>() == null) {
            Log.Warning("[heal] CompStormlight is null!");
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
            List<Hediff_MissingPart> missingParts = pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>()
                .OrderByDescending(h => h.Severity)
                .ToList();
            foreach (Hediff_MissingPart? injury in missingParts) {
                float cost = 175f; // More severe wounds cost more stormlight
                if (casterComp.Stormlight < cost) {
                    break;
                }

                pawn.health.hediffSet.hediffs.Remove(injury);
                casterComp.drawStormlight(cost);
                RadiantUtility.GiveRadiantXP(caster, 20f);
            }
        } else {
            Log.Message("Ideal level to low to heal missing part");
        }
    }

    private void healInjuries(Pawn pawn, Need_RadiantProgress radiantNeed, CompStormlight casterComp, Pawn caster) {
        List<Hediff_Injury> injuries = pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>()
            .OrderByDescending(h => h.Severity)
            .ToList();
        bool stillInjured = true;
        while (stillInjured && casterComp.HasStormlight) {
            stillInjured = false;
            foreach (Hediff_Injury? injury in injuries) {
                float cost = 1f;
                if (casterComp.Stormlight < cost) {
                    break;
                }

                float healAmount = 0.008f + radiantNeed.IdealLevel * 2f / 10f;
                injury.Heal(healAmount);
                casterComp.drawStormlight(cost);
                RadiantUtility.GiveRadiantXP(caster, 5f);
                if (injury.Severity > 0) stillInjured = true;
            }
        }
    }

    private void radiantHeal(Pawn pawn) {
        Pawn caster = parent.pawn;
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