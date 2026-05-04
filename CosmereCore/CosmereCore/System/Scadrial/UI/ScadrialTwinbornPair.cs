using UnityEngine;
using Verse;
using Cosmere.Core.Ability;
using Cosmere.Core.UI.Dock;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Gene;
using RimWorld;

namespace Cosmere.System.Scadrial.UI;

public sealed class ScadrialTwinbornPair : IDualInvestiturePair {
    private readonly Allomancer allomancer;
    private readonly Feruchemist feruchemist;

    public ScadrialTwinbornPair(Allomancer allomancer, Feruchemist feruchemist) {
        this.allomancer = allomancer;
        this.feruchemist = feruchemist;
    }

    public string MetalDefName => allomancer.metal.defName;
    public string MetalLabel => allomancer.metal.LabelCap;
    public Texture2D? MetalIcon => allomancer.metal.invertedIcon;

    public float PrimaryMax => allomancer.Max;
    public float PrimaryValue => allomancer.Value;
    public float? PrimaryTarget => allomancer.targetValue;
    public bool PrimaryActive => IsBurning(allomancer.pawn, MetalDefName);
    public string PrimaryActionLabel =>
        (PrimaryActive ? "CC_Dock_Twinborn_Stop" : "CC_Dock_Twinborn_Burn").Translate();

    public void TogglePrimary() {
        Pawn pawn = allomancer.pawn;
        if (pawn.abilities == null) return;
        List<RimWorld.Ability> all = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is AllomancyAbility a && a.metal.defName == MetalDefName) {
                Status next = a.atLeastBurning ? BurningStatus.Off : BurningStatus.Burning;
                a.UpdateStatus(next);
                return;
            }
        }
    }

    public float SecondaryMax {
        get {
            float max = 0f;
            List<IMetalmindSource> mms = feruchemist.metalminds;
            for (int i = 0; i < mms.Count; i++) max += mms[i].MaxAmount;
            return max;
        }
    }

    public float SecondaryValue {
        get {
            float v = 0f;
            List<IMetalmindSource> mms = feruchemist.metalminds;
            for (int i = 0; i < mms.Count; i++) v += mms[i].StoredAmount;
            return v;
        }
    }

    public string SecondaryActionLabel =>
        (feruchemist.isTapping
            ? "CC_Dock_Twinborn_Tap"
            : feruchemist.isStoring
                ? "CC_Dock_Twinborn_Store"
                : "CC_Dock_Twinborn_TapOrStore").Translate();

    public void ToggleSecondary() {
        if (feruchemist.isTapping) {
            feruchemist.targetValue = 75f;
            return;
        }

        if (feruchemist.isStoring) {
            feruchemist.Reset();
            return;
        }

        feruchemist.targetValue = 25f;
    }

    public string CompoundAbilityDefName => allomancer.metal.GetCompoundAbility()?.defName ?? string.Empty;

    private static bool IsBurning(Pawn pawn, string metalDefName) {
        if (pawn.abilities == null) return false;
        List<RimWorld.Ability> all = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is AllomancyAbility a && a.metal.defName == metalDefName) return a.atLeastBurning;
        }

        return false;
    }
}
