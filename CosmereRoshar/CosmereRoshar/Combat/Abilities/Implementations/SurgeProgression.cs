using Cosmere.Foundation;
using Cosmere.Roshar.Comp.Thing;
using Cosmere.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.Roshar.Combat.Abilities.Implementations;

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

    private void HealMissingParts(Pawn pawn, Surgebinder radiant, Stormlight stormlight, Pawn caster) {
        if (radiant is { currentIdeal: >= 3 }) {
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
                caster.skills.Learn(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower, .25f);
            }
        } else {
            Log.Message("Ideal level to low to heal missing part");
        }
    }

    private void HealInjuries(Pawn pawn, Surgebinder radiant, Stormlight stormlight, Pawn caster) {
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

                float healAmount = 0.008f + radiant.currentIdeal * 2f / 10f;
                injury.Heal(healAmount);
                stormlight.DrawStormlight(cost);
                caster.skills.Learn(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower, .05f);
                if (injury.Severity > 0) stillInjured = true;
            }
        }
    }

    private void RadiantHeal(Pawn pawn) {
        Pawn caster = parent.pawn;
        if (caster.TryGetComp(out Stormlight stormlight)) return;

        Surgebinder radiant = caster.genes.GetFirstGeneOfType<Surgebinder>();
        if (radiant == null) {
            Logger.Error("Pawn isn't a surgebinder!");
            return;
        }

        HealMissingParts(pawn, radiant, stormlight, caster);
        HealInjuries(pawn, radiant, stormlight, caster);
    }
}