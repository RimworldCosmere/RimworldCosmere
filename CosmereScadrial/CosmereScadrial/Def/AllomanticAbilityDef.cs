using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.Scadrial.Allomancy.Ability;
using Cosmere.Scadrial.Allomancy.Hediff;
using UnityEngine;
using Verse;

namespace Cosmere.Scadrial.Def;

public class AllomanticAbilityDef : AbilityDef, IMultiTypeHediff {
    public bool applyDragOnTarget = false;
    public HediffDef? dragHediff;
    public MetallicArtsMetalDef metal = null!;
    public float minSeverityForDrag = 1f;

    public override IEnumerable<string> ConfigErrors() {
        label ??= metal.label;
        foreach (string? error in base.ConfigErrors()) yield return error;

        if (!typeof(AbstractAllomancyAbility).IsAssignableFrom(abilityClass)) {
            yield return $"Invalid ability class {abilityClass}. Must inherit from {typeof(AbstractAllomancyAbility)}.";
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

        if (metal == null) yield return "metal is null";
    }

    public override void PostLoad() {
        if (string.IsNullOrEmpty(iconPath)) {
            string abilityName = defName.Replace("Cosmere_Scadrial_Ability_", "");
            LongEventHandler.ExecuteWhenFinished(() => {
                    uiIcon = ContentFinder<Texture2D>.Get($"UI/Icons/Abilities/{abilityName}", false) ??
                             metal.invertedIcon!;
                    disabledIcon = uiIcon.Overlay(ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff"));
                    pausedIcon = uiIcon.Overlay(ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Pause"));
                }
            );
        } else {
            base.PostLoad();
        }
    }
}