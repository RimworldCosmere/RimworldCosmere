using System.Collections.Generic;
using System.Linq;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Need;
using RimWorld;
using Verse;

namespace CosmereRoshar.Combat.Abilities.Implementations;

/// Surge regrowth plants
public class SurgeProgressionProperties : CompProperties_AbilityEffect {
    public float stormLightCost;

    public SurgeProgressionProperties() {
        compClass = typeof(SurgeProgression);
    }
}

public class SurgeProgression : CompAbilityEffect {
    public new SurgeProgressionProperties props => (SurgeProgressionProperties)base.props;

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


    private void HealFunction(Verse.Thing targetThing) {
        if (targetThing is not Pawn targetPawn) {
            return;
        }

        RadiantHeal(targetPawn);
    }

    private void HealMissingParts(Pawn pawn, RadiantProgress radiant, Stormlight stormlight, Pawn caster) {
        if (radiant is { idealLevel: >= 3 }) {
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

    private void HealInjuries(Pawn pawn, RadiantProgress radiant, Stormlight stormlight, Pawn caster) {
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

                float healAmount = 0.008f + radiant.idealLevel * 2f / 10f;
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

        RadiantProgress radiant = caster.needs.TryGetNeed<RadiantProgress>();
        if (radiant == null) {
            Log.Error("[HealSurge] need was null");
            return;
        }

        HealMissingParts(pawn, radiant, stormlight, caster);
        HealInjuries(pawn, radiant, stormlight, caster);
    }
}