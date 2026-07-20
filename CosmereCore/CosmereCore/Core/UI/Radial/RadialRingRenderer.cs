using System;
using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Radial;

public static class RadialRingRenderer {
    public static void DrawSystemRing(
        Vector2 center,
        IReadOnlyList<RadialSystem> systems,
        int hoveredIndex
    ) {
        DrawRing(
            center,
            systems.Count,
            (RadialLayout.SystemRingInner + RadialLayout.SystemRingOuter) / 2f,
            hoveredIndex,
            i => (systems[i].Label, systems[i].Icon, null, null, false, false, false, false, null, false)
        );
    }

    public static void DrawSubsectionRing(
        Vector2 center,
        RadialSystem system,
        int hoveredIndex
    ) {
        DrawRing(
            center,
            system.Subsections.Count,
            (RadialLayout.SubsectionRingInner + RadialLayout.SubsectionRingOuter) / 2f,
            hoveredIndex,
            i => (system.Subsections[i].Label, system.Subsections[i].Icon, system.Subsections[i].AccentColor, null,
                false, false, false, false, null, false)
        );
    }

    public static void DrawAbilityRing(
        Vector2 center,
        RadialSubsection subsection,
        ISystemSkin skin,
        int hoveredIndex
    ) {
        DrawRing(
            center,
            subsection.Leaves.Count,
            (RadialLayout.AbilityRingInner + RadialLayout.AbilityRingOuter) / 2f,
            hoveredIndex,
            i => {
                RadialLeaf leaf = subsection.Leaves[i];
                return (
                    leaf.Label,
                    leaf.Icon,
                    skin.AccentColor,
                    leaf.ReserveFraction,
                    leaf.IsActive,
                    leaf.IsFlaring,
                    leaf.IsSustained,
                    leaf.IsLocked,
                    leaf.LockReason,
                    leaf.HasInsufficientResources
                );
            }
        );
    }

    private static void DrawRing(
        Vector2 center,
        int count,
        float radius,
        int hoveredIndex,
        Func<int, (string label, Texture2D? icon, Color? tint, float? reserveFraction, bool isActive, bool isFlaring,
            bool isSustained, bool isLocked, string? lockReason, bool hasInsufficientResources)> getAt
    ) {
        if (count <= 0) return;

        Texture2D wedgeTex = RadialWedgeTex.Get(count);
        float arcDeg = 360f / count;
        float texDrawSize = RadialLayout.AbilityRingOuter * 2f;
        Rect texRect = new Rect(center.x - texDrawSize / 2f, center.y - texDrawSize / 2f, texDrawSize, texDrawSize);

        for (int i = 0; i < count; i++) {
            (string label, Texture2D? icon, Color? tint, float? reserveFraction, bool isActive, bool isFlaring,
                    bool isSustained, bool isLocked, string? lockReason, bool hasInsufficientResources) = getAt(i);

            Color bg = tint.HasValue
                ? Color.Lerp(DockPalette.Panel, tint.Value, 0.22f)
                : new Color(0.114f, 0.125f, 0.153f, 0.92f);
            bg.a = 0.92f;
            if (i == hoveredIndex) bg = Color.Lerp(bg, Color.white, 0.18f);
            if (isLocked) bg = new Color(bg.r, bg.g, bg.b, 0.4f);
            else if (hasInsufficientResources) bg = new Color(bg.r, bg.g, bg.b, 0.6f);

            Matrix4x4 prevMatrix = GUI.matrix;
            Verse.UI.RotateAroundPivot(i * arcDeg, center);
            Color prevColor = GUI.color;
            if (isFlaring) {
                GUI.color = new Color(DockPalette.Flare.r, DockPalette.Flare.g, DockPalette.Flare.b, 0.35f);
                GUI.DrawTexture(texRect.ExpandedBy(5f), wedgeTex);
            }
            else if (isActive) {
                GUI.color = new Color(DockPalette.HotLabel.r, DockPalette.HotLabel.g, DockPalette.HotLabel.b, 0.3f);
                GUI.DrawTexture(texRect.ExpandedBy(4f), wedgeTex);
            }

            GUI.color = bg;
            GUI.DrawTexture(texRect, wedgeTex);
            GUI.color = prevColor;
            GUI.matrix = prevMatrix;

            Vector2 mid = RadialLayout.WedgeMidpoint(i, count, radius, center);
            Rect iconRect = new Rect(mid.x - 16f, mid.y - 24f, 32f, 32f);
            if (icon != null) {
                Color originalGui = GUI.color;
                GUI.color = isLocked ? new Color(1f, 1f, 1f, 0.4f) : Color.white;
                GUI.DrawTexture(iconRect, icon);
                GUI.color = originalGui;
            }

            float chordWidth = 2f * radius * Mathf.Sin(arcDeg * 0.5f * Mathf.Deg2Rad) - 10f;
            Rect labelRect = new Rect(mid.x - chordWidth / 2f, iconRect.yMax + 2f, chordWidth, 14f);
            Color labelColor = isLocked ? new Color(0.36f, 0.42f, 0.46f) : new Color(0.81f, 0.85f, 0.87f);
            UIText.EllipsisLabel(labelRect, label, GameFont.Tiny, TextAnchor.MiddleCenter, labelColor);

            if (isSustained) {
                Rect dot = new Rect(mid.x + 20f, mid.y - 24f, 5f, 5f);
                Widgets.DrawBoxSolid(dot, DockPalette.HotLabel);
            }

            if (isLocked) {
                Rect lockRect = new Rect(mid.x - 5f, labelRect.yMax + 1f, 10f, 10f);
                using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, DockPalette.Flare))
                    Widgets.Label(lockRect, "x");
            }

            if (reserveFraction.HasValue && !isLocked) {
                Rect barRect = new Rect(mid.x - 18f, labelRect.yMax + 2f, 36f, 2f);
                Widgets.DrawBoxSolid(barRect, new Color(0f, 0f, 0f, 0.6f));
                Widgets.DrawBoxSolid(new Rect(barRect.x, barRect.y, barRect.width * Mathf.Clamp01(reserveFraction.Value), 2f), DockPalette.HotLabel);
                if (hasInsufficientResources) {
                    Rect pctRect = new Rect(mid.x - 18f, barRect.yMax + 1f, 36f, 10f);
                    using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, new Color(1f, 0.55f, 0.55f)))
                        Widgets.Label(pctRect, $"{Mathf.RoundToInt(reserveFraction.Value * 100f)}%");
                }
            }
        }
    }
}