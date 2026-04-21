using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Radial;

public static class RadialRingRenderer {
    private const float WedgeSize = 64f;
    private const float IconSize = 32f;

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
            i => (system.Subsections[i].Label, system.Subsections[i].Icon, (Color?)system.Subsections[i].AccentColor, null, false, false, false, false, null, false)
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
                    (Color?)skin.AccentColor,
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
        global::System.Func<int, (string label, Texture2D? icon, Color? tint, float? reserveFraction, bool isActive, bool isFlaring, bool isSustained, bool isLocked, string? lockReason, bool hasInsufficientResources)> getAt
    ) {
        if (count <= 0) return;

        for (int i = 0; i < count; i++) {
            Vector2 mid = RadialLayout.WedgeMidpoint(i, count, radius, center);
            Rect wedgeRect = new Rect(mid.x - WedgeSize / 2f, mid.y - WedgeSize / 2f, WedgeSize, WedgeSize);

            (string label, Texture2D? icon, Color? tint, float? reserveFraction, bool isActive, bool isFlaring, bool isSustained, bool isLocked, string? lockReason, bool hasInsufficientResources) =
                getAt(i);

            Color bg = tint ?? new Color(0.1f, 0.1f, 0.14f, 0.75f);
            if (i == hoveredIndex) bg = Color.Lerp(bg, Color.white, 0.25f);
            if (isLocked) bg = new Color(bg.r, bg.g, bg.b, 0.35f);
            if (hasInsufficientResources && !isLocked) bg = new Color(bg.r, bg.g, bg.b, bg.a * 0.55f);

            Widgets.DrawBoxSolid(wedgeRect, bg);
            Widgets.DrawBox(wedgeRect);

            if (isActive) {
                Widgets.DrawBoxSolid(wedgeRect.ExpandedBy(2f), new Color(1f, 0.7f, 0.2f, 0.25f));
            }
            if (isFlaring) {
                using (new TextBlock(Color.red)) {
                    Widgets.DrawBox(wedgeRect.ExpandedBy(2f), 2);
                }
            }

            Rect iconRect = new Rect(
                wedgeRect.center.x - IconSize / 2f,
                wedgeRect.y + 4f,
                IconSize,
                IconSize
            );
            if (icon != null) {
                Color originalGui = GUI.color;
                GUI.color = isLocked ? new Color(1f, 1f, 1f, 0.4f) : Color.white;
                GUI.DrawTexture(iconRect, icon);
                GUI.color = originalGui;
            }

            if (isSustained) {
                Rect dot = new Rect(wedgeRect.xMax - 8f, wedgeRect.y + 2f, 5f, 5f);
                Widgets.DrawBoxSolid(dot, new Color(1f, 0.8f, 0.3f));
            }
            if (isLocked) {
                Rect lockIcon = new Rect(wedgeRect.xMax - 12f, wedgeRect.yMax - 12f, 10f, 10f);
                using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, Color.red))
                    Widgets.Label(lockIcon, "x");
            }

            Rect labelRect = new Rect(wedgeRect.x, wedgeRect.yMax - 16f, wedgeRect.width, 14f);
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, Color.white))
                Widgets.Label(labelRect, label);

            if (reserveFraction.HasValue && !isLocked) {
                Rect barRect = new Rect(wedgeRect.x + 4f, wedgeRect.yMax - 3f, wedgeRect.width - 8f, 2f);
                Widgets.DrawBoxSolid(barRect, new Color(0.2f, 0.2f, 0.2f, 0.8f));
                Rect fill = new Rect(barRect.x, barRect.y, barRect.width * Mathf.Clamp01(reserveFraction.Value), barRect.height);
                Widgets.DrawBoxSolid(fill, new Color(1f, 0.9f, 0.4f, 0.9f));

                if (hasInsufficientResources) {
                    Rect pctRect = new Rect(wedgeRect.x + 2f, wedgeRect.y + 2f, 14f, 10f);
                    using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, new Color(1f, 0.55f, 0.55f)))
                        Widgets.Label(pctRect, $"{Mathf.RoundToInt(reserveFraction.Value * 100f)}%");
                }
            }
        }
    }
}
