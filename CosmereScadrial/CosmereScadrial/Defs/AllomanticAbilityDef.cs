using System.Collections.Generic;
using CosmereScadrial.Abilities.Allomancy;
using CosmereScadrial.Abilities.Allomancy.Hediffs;
using RimWorld;
using UnityEngine;
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
        public MetallicArtsMetalDef metal;
        public float minSeverityForDrag = 1f;
        public bool toggleable;

        public override TaggedString LabelCap {
            get {
                if (label.NullOrEmpty()) return (TaggedString)(string)null;
                if (cachedLabelCap.NullOrEmpty()) cachedLabelCap = (TaggedString)GenText.ToTitleCaseSmart(label);

                return cachedLabelCap;
            }
        }

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

            if (!typeof(AbstractAbility).IsAssignableFrom(abilityClass)) {
                yield return $"Invalid ability class {abilityClass}. Must inherit from {typeof(AbstractAbility)}.";
            }

            if (hediff != null && !typeof(AllomanticHediff).IsAssignableFrom(hediff.hediffClass)) {
                yield return "hediff.hediffClass is not AllomanticHediff";
            }

            if (hediffFriendly != null && !typeof(AllomanticHediff).IsAssignableFrom(hediffFriendly.hediffClass)) {
                yield return "hediffFriendly.hediffClass is not AllomanticHediff";
            }

            if (hediffHostile != null && !typeof(AllomanticHediff).IsAssignableFrom(hediffHostile.hediffClass)) {
                yield return "hediffHostile.hediffClass is not AllomanticHediff";
            }

            if (metal == null) {
                yield return "metal is null";
            }
        }

        public override void PostLoad() {
            LongEventHandler.ExecuteWhenFinished(() =>
                uiIcon = ContentFinder<Texture2D>.Get($"UI/Icons/Genes/Investiture/Allomancy/{metal.defName}"));
        }
    }
}