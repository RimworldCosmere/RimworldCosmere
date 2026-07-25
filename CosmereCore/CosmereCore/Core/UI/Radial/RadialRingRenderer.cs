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
            i => {
                RadialSubsection subsection = system.Subsections[i];
                // A collapsed metal never reaches the ability ring, so its lit
                // state has to come from the leaves it stands in for.
                bool anyActive = false;
                bool anyFlaring = false;
                for (int leafIndex = 0; leafIndex < subsection.Leaves.Count; leafIndex++) {
                    anyActive |= subsection.Leaves[leafIndex].IsActive;
                    anyFlaring |= subsection.Leaves[leafIndex].IsFlaring;
                }

                return (
                    subsection.Label,
                    subsection.Icon,
                    subsection.AccentColor,
                    (float?)null,
                    anyActive,
                    anyFlaring,
                    false,
                    false,
                    (string?)null,
                    false
                );
            }
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

        Color backingPrev = GUI.color;
        GUI.color = new Color(0.16f, 0.175f, 0.205f, 0.97f);
        GUI.DrawTexture(texRect, RadialWedgeTex.Backing());
        GUI.color = backingPrev;

        for (int i = 0; i < count; i++) {
            (string label, Texture2D? icon, Color? tint, float? reserveFraction, bool isActive, bool isFlaring,
                    bool isSustained, bool isLocked, string? lockReason, bool hasInsufficientResources) = getAt(i);

            Color bg = tint.HasValue
                ? Color.Lerp(DockPalette.Panel, tint.Value, 0.18f)
                : new Color(0.16f, 0.175f, 0.205f);
            bg.a = 0.97f;
            // A lit metal keeps a warm fill of its own so it reads as burning
            // even when the cursor is elsewhere; hovering still wins over it.
            if (isActive) bg = new Color(0.34f, 0.24f, 0.10f, 0.97f);
            if (i == hoveredIndex) bg = new Color(0.42f, 0.31f, 0.14f, 0.97f);
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

            if (i == hoveredIndex && !isLocked) {
                GUI.color = new Color(DockPalette.HotLabel.r, DockPalette.HotLabel.g, DockPalette.HotLabel.b, 0.85f);
                GUI.DrawTexture(texRect, RadialWedgeTex.InnerEdge(count));
            }

            GUI.color = prevColor;
            GUI.matrix = prevMatrix;

            Matrix4x4 sepMatrix = GUI.matrix;
            Verse.UI.RotateAroundPivot(i * arcDeg - arcDeg / 2f, center);
            Color sepColor = GUI.color;
            GUI.color = new Color(DockPalette.Border.r, DockPalette.Border.g, DockPalette.Border.b, 0.5f);
            GUI.DrawTexture(
                new Rect(
                    center.x - 0.5f,
                    center.y - RadialLayout.AbilityRingOuter,
                    1f,
                    RadialLayout.AbilityRingOuter - RadialLayout.AbilityRingInner
                ),
                Verse.BaseContent.WhiteTex
            );
            GUI.color = sepColor;
            GUI.matrix = sepMatrix;

            Vector2 iconMid = RadialLayout.WedgeMidpoint(i, count, RadialLayout.IconBandRadius, center);
            Rect iconRect = new Rect(iconMid.x - 16f, iconMid.y - 16f, 32f, 32f);
            if (icon != null) {
                Color originalGui = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 0.45f);
                GUI.DrawTexture(iconRect.ExpandedBy(5f), RadialWedgeTex.Disc());
                GUI.color = isLocked ? new Color(1f, 1f, 1f, 0.4f) : Color.white;
                GUI.DrawTexture(iconRect, icon);
                GUI.color = originalGui;
            }

            if (isSustained) {
                Rect dot = new Rect(iconRect.xMax + 2f, iconRect.y + 2f, 5f, 5f);
                Widgets.DrawBoxSolid(dot, DockPalette.HotLabel);
            }

            if (reserveFraction.HasValue && !isLocked && hasInsufficientResources) {
                Rect pctRect = new Rect(iconMid.x - 18f, iconRect.yMax + 3f, 36f, Text.LineHeightOf(GameFont.Tiny));
                using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, new Color(1f, 0.55f, 0.55f)))
                    Widgets.Label(pctRect, $"{Mathf.RoundToInt(reserveFraction.Value * 100f)}%");
            }

            if (isLocked) {
                Rect lockRect = new Rect(iconMid.x - 5f, iconRect.yMax + 3f, 10f, Text.LineHeightOf(GameFont.Tiny));
                using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, DockPalette.Flare))
                    Widgets.Label(lockRect, "x");
            }

            float tinyH = Text.LineHeightOf(GameFont.Tiny);
            Vector2 labelMid = RadialLayout.WedgeMidpoint(i, count, RadialLayout.LabelBandRadius, center);
            float labelWidth = RadialLayout.ChordWidthAt(RadialLayout.LabelBandRadius, count) - 8f;
            Rect labelRect = new Rect(labelMid.x - labelWidth / 2f, labelMid.y - tinyH / 2f, labelWidth, tinyH);
            bool flareArmed = i == hoveredIndex
                && !isLocked
                && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            Color labelColor = isLocked
                ? new Color(0.36f, 0.42f, 0.46f)
                : flareArmed
                    ? DockPalette.Flare
                    : i == hoveredIndex
                        ? Color.white
                        : new Color(0.81f, 0.85f, 0.87f);
            UIText.EllipsisLabel(labelRect, label, GameFont.Tiny, TextAnchor.MiddleCenter, labelColor);
        }
    }
}