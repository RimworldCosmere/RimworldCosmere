using Cosmere.Core.Framework;
using Cosmere.Core.Hediff;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Def;

public class AbilityDef : RimWorld.AbilityDef, IMultiTypeHediff {
    public ThingDef? activeMote;
    public float asleepStrengthFactor = .5f;
    public bool autoUseWhileDowned = false;
    public bool autoUseWhileInjured = false;
    public float beuPerTick = CoreBreathConstants.DefaultBreathEquivalentUnitsPerTick;
    public bool canUseWhileAsleep = false;
    public bool canUseWhileDowned = false;
    public Texture2D disabledIcon = BaseContent.BadTex;
    public float downedStrengthFactor = .25f;
    public HediffDef? hediff;
    public HediffDef? hediffFriendly;
    public HediffDef? hediffHostile;
    public float hediffSeverityFactor = 1f;
    public bool isAutocast = false;
    public int maxPower = 1;
    public Texture2D pausedIcon = BaseContent.BadTex;
    public bool toggleable = false;

    public override TaggedString LabelCap {
        get {
            if (label.NullOrEmpty()) return TaggedString.Empty;
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
    }

    public override void PostLoad() {
        if (string.IsNullOrEmpty(iconPath)) {
            string abilityName = this.ParseDefName().ElementAt(3);
            LongEventHandler.ExecuteWhenFinished(() => {
                uiIcon = ContentFinder<Texture2D>.Get($"UI/Icons/Abilities/{abilityName}", false) ??
                         BaseContent.BadTex;
                disabledIcon = uiIcon.Overlay(ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff"));
                pausedIcon = uiIcon.Overlay(ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Pause"));
            }
            );
        }
        else {
            base.PostLoad();
        }
    }
}