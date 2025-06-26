using System.Collections.Generic;
using CosmereFramework.Extension;
using CosmereScadrial.Ability.Allomancy;
using CosmereScadrial.Ability.Allomancy.Hediff;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Def;

public class AllomanticAbilityDef : AbilityDef, IMultiTypeHediff {
    public bool applyDragOnTarget = false;
    public float beuPerTick = 0.1f / GenTicks.TicksPerRealSecond;
    public ThingDef? burningMote;
    public int burningMoteInterval = GenTicks.TickRareInterval;
    public bool canBurnWhileAsleep = false;
    public bool canBurnWhileDowned = false;
    public bool canFlare = true;
    public Texture2D disabledIcon = BaseContent.BadTex;
    public HediffDef? dragHediff;
    public HediffDef? hediff;
    public HediffDef? hediffFriendly;
    public HediffDef? hediffHostile;
    public float hediffSeverityFactor = 1f;
    public MetallicArtsMetalDef metal = null!;
    public float minSeverityForDrag = 1f;
    public bool toggleable = false;

    public override TaggedString LabelCap {
        get {
            if (label.NullOrEmpty()) return (TaggedString)(string)null!;
            if (cachedLabelCap.NullOrEmpty()) cachedLabelCap = (TaggedString)GenText.ToTitleCaseSmart(label);

            return cachedLabelCap;
        }
    }

    public HediffDef? GetHediff() {
        return hediff;
    }

    public HediffDef? GetFriendlyHediff() {
        return hediffFriendly;
    }

    public HediffDef? GetHostileHediff() {
        return hediffHostile;
    }

    public override IEnumerable<string> ConfigErrors() {
        label ??= metal.label;
        foreach (string? error in base.ConfigErrors()) {
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
        if (string.IsNullOrEmpty(iconPath)) {
            string abilityName = defName.Replace("Cosmere_Scadrial_Ability_", "");
            LongEventHandler.ExecuteWhenFinished(() => {
                uiIcon = ContentFinder<Texture2D>.Get($"UI/Icons/Abilities/{abilityName}", false) ??
                         metal.invertedIcon;
                disabledIcon = uiIcon.Overlay(ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff"));
            });
        } else {
            base.PostLoad();
        }
    }
}