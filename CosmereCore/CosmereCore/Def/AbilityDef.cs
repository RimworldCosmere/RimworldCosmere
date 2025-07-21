using System.Collections.Generic;
using Cosmere.Framework.Extension;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Def;

public class AbilityDef : RimWorld.AbilityDef, IMultiTypeHediff {
    public ThingDef? activeMote;
    public bool autoUseWhileDownedByDefault = true;
    public float beuPerTick = Constants.DefaultBreathEquivalentUnitsPerTick;
    public bool canUseWhileAsleep = false;
    public bool canUseWhileDowned = false;
    public Texture2D disabledIcon = BaseContent.BadTex;
    public HediffDef? hediff;
    public HediffDef? hediffFriendly;
    public HediffDef? hediffHostile;
    public float hediffSeverityFactor = 1f;
    public int maxPower = 1;
    public Texture2D pausedIcon = BaseContent.BadTex;
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
        foreach (string? error in base.ConfigErrors()) yield return error;

        // Until i can figure out why the generic makes this check dumb, gonna skip it
        /*if (!abilityClass.InheritsOrImplements(typeof(AbstractAbility))) {
            yield return
                $"Invalid ability class {abilityClass}. Must inherit from {typeof(AbstractAbility)}.";
        }*/
    }

    public override void PostLoad() {
        if (string.IsNullOrEmpty(iconPath)) {
            string abilityName = defName.Replace("Cosmere_Scadrial_Ability_", "");
            LongEventHandler.ExecuteWhenFinished(() => {
                    uiIcon = ContentFinder<Texture2D>.Get($"UI/Icons/Abilities/{abilityName}", false) ??
                             BaseContent.BadTex;
                    disabledIcon = uiIcon.Overlay(ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff"));
                    pausedIcon = uiIcon.Overlay(ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Pause"));
                }
            );
        } else {
            base.PostLoad();
        }
    }
}