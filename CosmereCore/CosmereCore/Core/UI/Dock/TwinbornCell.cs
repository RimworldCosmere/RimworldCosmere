using Cosmere.Core.UI.Skin;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;
using AbilityStatus = Cosmere.Core.Ability.Status;

namespace Cosmere.Core.UI.Dock;

public static class TwinbornCell {
    private const float CellHeight = 56f;
    private const float CellSpacing = 4f;
    private const float ButtonRowHeight = 20f;
    private const float ButtonWidth = 54f;

    public static float Height => CellHeight;
    public static float Spacing => CellSpacing;

    public static void Draw(
        Rect cellRect,
        Pawn pawn,
        TwinbornPair pair,
        ISystemSkin allomancySkin,
        ISystemSkin feruchemySkin
    ) {
        Widgets.DrawBoxSolid(cellRect, new Color(0.02f, 0.02f, 0.02f, 0.6f));
        Widgets.DrawBox(cellRect);

        float iconSize = 28f;
        Rect iconRect = new Rect(cellRect.x + 4f, cellRect.y + 4f, iconSize, iconSize);
        Texture2D? icon = pair.Allomancer.metal.invertedIcon;
        if (icon != null) GUI.DrawTexture(iconRect, icon);

        Rect labelRect = new Rect(iconRect.xMax + 4f, cellRect.y + 2f, cellRect.width - iconSize - 12f, 14f);
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(labelRect, pair.Allomancer.metal.LabelCap);

        Rect topBar = new Rect(iconRect.xMax + 4f, cellRect.y + 16f, cellRect.width - iconSize - 12f, 10f);
        HorizontalBar.Draw(
            topBar,
            pair.Allomancer.Max > 0f ? pair.Allomancer.Value / pair.Allomancer.Max : 0f,
            pair.Allomancer.Max > 0f ? pair.Allomancer.targetValue / pair.Allomancer.Max : null,
            allomancySkin.BarBackgroundColor,
            allomancySkin.BarFillColor,
            new Color(1f, 1f, 1f, 0.8f)
        );

        float fMax = 0f, fVal = 0f;
        List<IMetalmindSource> mms = pair.Feruchemist.metalminds;
        for (int j = 0; j < mms.Count; j++) {
            fMax += mms[j].maxAmount;
            fVal += mms[j].storedAmount;
        }
        Rect bottomBar = new Rect(iconRect.xMax + 4f, topBar.yMax + 2f, topBar.width, 10f);
        HorizontalBar.Draw(
            bottomBar,
            fMax > 0f ? fVal / fMax : 0f,
            null,
            feruchemySkin.BarBackgroundColor,
            feruchemySkin.BarFillColor,
            new Color(1f, 1f, 1f, 0.8f)
        );

        float btnY = bottomBar.yMax + 4f;
        Rect burnRect = new Rect(cellRect.x + 4f, btnY, ButtonWidth, ButtonRowHeight);
        Rect tapRect = new Rect(burnRect.xMax + 4f, btnY, ButtonWidth, ButtonRowHeight);
        Rect cpdRect = new Rect(tapRect.xMax + 4f, btnY, ButtonWidth, ButtonRowHeight);

        bool isBurning = IsAllomancyBurning(pawn, pair.MetalDefName);
        if (Widgets.ButtonText(burnRect, (isBurning ? "CC_Dock_Twinborn_Stop" : "CC_Dock_Twinborn_Burn").Translate())) {
            ToggleAllomancyBurn(pawn, pair.MetalDefName);
        }

        string ferLabel = (pair.Feruchemist.isTapping
            ? "CC_Dock_Twinborn_Tap"
            : pair.Feruchemist.isStoring
                ? "CC_Dock_Twinborn_Store"
                : "CC_Dock_Twinborn_TapOrStore").Translate();
        if (Widgets.ButtonText(tapRect, ferLabel)) {
            ToggleFeruchemyDirection(pair.Feruchemist);
            Event.current?.Use();
        }

        AbilityDef? cpdDef = DefDatabase<AbilityDef>.GetNamedSilentFail(
            "Cosmere_Scadrial_Ability_Compound" + pair.MetalDefName
        );
        RimWorld.Ability? cpdAbility = cpdDef != null ? pawn.abilities?.GetAbility(cpdDef) : null;
        bool cpdEnabled = cpdAbility != null && cpdAbility.CanCast;

        Color orig = GUI.color;
        if (!cpdEnabled) GUI.color = new Color(1f, 1f, 1f, 0.4f);
        if (Widgets.ButtonText(cpdRect, "CC_Dock_Twinborn_Compound".Translate(), active: cpdEnabled) && cpdEnabled) {
            cpdAbility!.QueueCastingJob(pawn, LocalTargetInfo.Invalid);
        }
        GUI.color = orig;
    }

    private static bool IsAllomancyBurning(Pawn pawn, string metalDefName) {
        if (pawn.abilities == null) return false;
        List<RimWorld.Ability> all = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is AllomancyAbility a && a.metal.defName == metalDefName) return a.atLeastBurning;
        }
        return false;
    }

    private static void ToggleAllomancyBurn(Pawn pawn, string metalDefName) {
        if (pawn.abilities == null) return;
        List<RimWorld.Ability> all = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is AllomancyAbility a && a.metal.defName == metalDefName) {
                AbilityStatus next = a.atLeastBurning ? BurningStatus.Off : BurningStatus.Burning;
                a.UpdateStatus(next);
                Event.current?.Use();
                return;
            }
        }
    }

    private static void ToggleFeruchemyDirection(Feruchemist gene) {
        if (gene.isTapping) {
            gene.targetValue = 75f;
            return;
        }
        if (gene.isStoring) {
            gene.Reset();
            return;
        }
        gene.targetValue = 25f;
    }
}
