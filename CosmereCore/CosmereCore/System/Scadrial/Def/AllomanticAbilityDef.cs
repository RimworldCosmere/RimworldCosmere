using Cosmere.Core.Def;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Def;

public class AllomanticAbilityDef : AbilityDef {
    public bool applyDragOnTarget = false;
    public HediffDef? dragHediff;
    public bool isCompound;
    public MetallicArtsMetalDef metal = null!;
    public float minSeverityForDrag = 1f;

    public override IEnumerable<string> ConfigErrors() {
        label ??= metal.label;
        foreach (string? error in base.ConfigErrors()) yield return error;

        if (!typeof(AllomancyAbility).IsAssignableFrom(abilityClass)) {
            yield return $"Invalid ability class {abilityClass}. Must inherit from {typeof(AllomancyAbility)}.";
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

        if (metal is null) yield return "metal is null";
    }

    public override void PostLoad() {
        if (string.IsNullOrEmpty(iconPath)) {
            string abilityName = defName.Replace("Cosmere_Scadrial_Ability_", string.Empty);
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
