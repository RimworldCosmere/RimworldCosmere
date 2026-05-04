using Cosmere.Core.UI.Skin;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

public static class DualInvestitureCell {
    private const float CellHeight = 56f;
    private const float CellSpacing = 4f;
    private const float ButtonRowHeight = 20f;
    private const float ButtonWidth = 54f;

    public static float Height => CellHeight;
    public static float Spacing => CellSpacing;

    public static void Draw(
        Rect cellRect,
        Pawn pawn,
        IDualInvestiturePair pair,
        ISystemSkin allomancySkin,
        ISystemSkin feruchemySkin
    ) {
        Widgets.DrawBoxSolid(cellRect, new Color(0.02f, 0.02f, 0.02f, 0.6f));
        Widgets.DrawBox(cellRect);

        float iconSize = 28f;
        Rect iconRect = new Rect(cellRect.x + 4f, cellRect.y + 4f, iconSize, iconSize);
        Texture2D? icon = pair.MetalIcon;
        if (icon != null) GUI.DrawTexture(iconRect, icon);

        Rect labelRect = new Rect(iconRect.xMax + 4f, cellRect.y + 2f, cellRect.width - iconSize - 12f, 14f);
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(labelRect, pair.MetalLabel);

        Rect topBar = new Rect(iconRect.xMax + 4f, cellRect.y + 16f, cellRect.width - iconSize - 12f, 10f);
        HorizontalBar.Draw(
            topBar,
            pair.PrimaryMax > 0f ? pair.PrimaryValue / pair.PrimaryMax : 0f,
            pair.PrimaryMax > 0f && pair.PrimaryTarget.HasValue ? pair.PrimaryTarget.Value / pair.PrimaryMax : null,
            allomancySkin.BarBackgroundColor,
            allomancySkin.BarFillColor,
            new Color(1f, 1f, 1f, 0.8f)
        );

        Rect bottomBar = new Rect(iconRect.xMax + 4f, topBar.yMax + 2f, topBar.width, 10f);
        HorizontalBar.Draw(
            bottomBar,
            pair.SecondaryMax > 0f ? pair.SecondaryValue / pair.SecondaryMax : 0f,
            null,
            feruchemySkin.BarBackgroundColor,
            feruchemySkin.BarFillColor,
            new Color(1f, 1f, 1f, 0.8f)
        );

        float btnY = bottomBar.yMax + 4f;
        Rect burnRect = new Rect(cellRect.x + 4f, btnY, ButtonWidth, ButtonRowHeight);
        Rect tapRect = new Rect(burnRect.xMax + 4f, btnY, ButtonWidth, ButtonRowHeight);
        Rect cpdRect = new Rect(tapRect.xMax + 4f, btnY, ButtonWidth, ButtonRowHeight);

        if (Widgets.ButtonText(burnRect, pair.PrimaryActionLabel)) {
            pair.TogglePrimary();
            Event.current?.Use();
        }

        if (Widgets.ButtonText(tapRect, pair.SecondaryActionLabel)) {
            pair.ToggleSecondary();
            Event.current?.Use();
        }

        AbilityDef? cpdDef = DefDatabase<AbilityDef>.GetNamedSilentFail(pair.CompoundAbilityDefName);
        RimWorld.Ability? cpdAbility = cpdDef != null ? pawn.abilities?.GetAbility(cpdDef) : null;
        bool cpdEnabled = cpdAbility != null && cpdAbility.CanCast;

        Color orig = GUI.color;
        if (!cpdEnabled) GUI.color = new Color(1f, 1f, 1f, 0.4f);
        if (Widgets.ButtonText(cpdRect, "CC_Dock_Twinborn_Compound".Translate(), active: cpdEnabled) && cpdEnabled) {
            cpdAbility!.QueueCastingJob(pawn, LocalTargetInfo.Invalid);
        }

        GUI.color = orig;
    }
}
