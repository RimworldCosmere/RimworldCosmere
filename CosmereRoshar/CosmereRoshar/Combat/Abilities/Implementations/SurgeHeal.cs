using System.Collections.Generic;
using System.Linq;
using CosmereRoshar.Comp;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Comps;
using CosmereRoshar.Need;
using RimWorld;
using Verse;

namespace CosmereRoshar.Combat.Abilities.Implementations;

/// Surge regrowth plants
public class CompPropertiesAbilitySurgeHeal : CompProperties_AbilityEffect {
    public float stormLightCost;

    public CompPropertiesAbilitySurgeHeal() {
        compClass = typeof(CompAbilityEffectSurgeHeal);
    }
}

public class CompAbilityEffectSurgeHeal : CompAbilityEffect {
    public new CompPropertiesAbilitySurgeHeal props => ((AbilityComp)this).props as CompPropertiesAbilitySurgeHeal;

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

        if (caster.GetComp<Stormlight>() == null) {
            Log.Warning("[heal] Stormlight is null!");
            return;
        }


        // 3) heal target
        HealFunction(target.Thing);
    }


    private void HealFunction(Thing targetThing) {
        Map map = targetThing.Map;

        Pawn targetPawn = targetThing as Pawn;
        if (targetPawn == null) {
            return;
        }

        RadiantHeal(targetPawn);
    }

    private void HealMissingParts(Pawn pawn, NeedRadiantProgress radiantNeed, Stormlight stormlight, Pawn caster) {
        if (radiantNeed is { idealLevel: >= 3 }) {
            List<Hediff_MissingPart> missingParts = pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>()
                .OrderByDescending(h => h.Severity)
                .ToList();
            foreach (Hediff_MissingPart? injury in missingParts) {
                float cost = 175f; // More severe wounds cost more stormlight
                if (stormlight.currentStormlight < cost) {
                    break;
                }

                pawn.health.hediffSet.hediffs.Remove(injury);
                stormlight.DrawStormlight(cost);
                RadiantUtility.GiveRadiantXp(caster, 20f);
            }
        } else {
            Log.Message("Ideal level to low to heal missing part");
        }
    }

    private void HealInjuries(Pawn pawn, NeedRadiantProgress radiantNeed, Stormlight stormlight, Pawn caster) {
        List<Hediff_Injury> injuries = pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>()
            .OrderByDescending(h => h.Severity)
            .ToList();
        bool stillInjured = true;
        while (stillInjured && stormlight.hasStormlight) {
            stillInjured = false;
            foreach (Hediff_Injury? injury in injuries) {
                float cost = 1f;
                if (stormlight.currentStormlight < cost) {
                    break;
                }

                float healAmount = 0.008f + radiantNeed.idealLevel * 2f / 10f;
                injury.Heal(healAmount);
                stormlight.DrawStormlight(cost);
                RadiantUtility.GiveRadiantXp(caster, 5f);
                if (injury.Severity > 0) stillInjured = true;
            }
        }
    }

    private void RadiantHeal(Pawn pawn) {
        Pawn caster = parent.pawn;
        if (caster.TryGetComp(out Stormlight stormlight)) return;
        
        NeedRadiantProgress radiantNeed = caster.needs.TryGetNeed<NeedRadiantProgress>();
        if (radiantNeed == null) {
            Log.Error("[HealSurge] need was null");
            return;
        }

        HealMissingParts(pawn, radiantNeed, stormlight, caster);
        HealInjuries(pawn, radiantNeed, stormlight, caster);
    }
}