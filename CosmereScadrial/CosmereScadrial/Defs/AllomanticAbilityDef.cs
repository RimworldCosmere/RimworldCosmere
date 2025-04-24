using System.Collections.Generic;
using CosmereScadrial.Abilities.Allomancy;
using CosmereScadrial.Abilities.Allomancy.Hediffs;
using RimWorld;
using Verse;

namespace CosmereScadrial.Defs {
    public class AllomanticAbilityDef : AbilityDef, MultiTypeHediff {
        public bool applyDragOnTarget = false;
        public float beuPerTick = 0.1f / GenTicks.TicksPerRealSecond;
        public HediffDef dragHediff;


        public HediffDef hediff;
        public HediffDef hediffFriendly;
        public HediffDef hediffHostile;
        public float hediffSeverityFactor = 1f;
        public MaintenanceDef maintenance;
        public MetallicArtsMetalDef metal;
        public float minSeverityForDrag = 1f;
        public bool toggleable;

        public HediffDef getHediff() {
            return hediff;
        }

        public HediffDef getFriendlyHediff() {
            return hediffFriendly;
        }

        public HediffDef getHostileHediff() {
            return hediffHostile;
        }

        public override IEnumerable<string> ConfigErrors() {
            foreach (var error in base.ConfigErrors()) {
                yield return error;
            }

            if (abilityClass.IsAssignableFrom(typeof(AbstractAbility))) {
                yield return $"Invalid ability class {abilityClass}. Must inherit from {typeof(AbstractAbility)}.";
            }

            if (hediff != null && hediff.hediffClass != typeof(AllomanticHediff)) {
                yield return "hediff.hediffClass is not AllomanticHediff";
            }

            if (hediffFriendly != null && hediffFriendly.hediffClass != typeof(AllomanticHediff)) {
                yield return "hediffFriendly.hediffClass is not AllomanticHediff";
            }

            if (hediffHostile != null && hediffHostile.hediffClass != typeof(AllomanticHediff)) {
                yield return "hediffHostile.hediffClass is not AllomanticHediff";
            }

            if (metal == null) {
                yield return "metal is null";
            }
        }
    }

    public class MaintenanceDef {
        public float followRadius = 10f;
        public JobDef jobDef = JobDefOf.Cosmere_Job_MaintainAllomanticTarget;
    }
}