using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Radial;

public static class RadialCenterPreview {
    private const float CenterRadius = 40f;

    public static void Draw(Vector2 center, RadialLeaf? hoveredLeaf) {
        Rect disc = new Rect(center.x - CenterRadius, center.y - CenterRadius, CenterRadius * 2f, CenterRadius * 2f);
        Widgets.DrawBoxSolid(disc, new Color(0.05f, 0.05f, 0.08f, 0.9f));
        Widgets.DrawBox(disc);

        if (hoveredLeaf == null) {
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.8f)))
                Widgets.Label(disc, "CC_Radial_Hover_Prompt".Translate());
            return;
        }

        float y = disc.y + 6f;
        Rect labelRect = new Rect(disc.x, y, disc.width, 16f);
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, Color.white))
            Widgets.Label(labelRect, hoveredLeaf.Label);
        y += 18f;

        if (hoveredLeaf.CostHint != null) {
            Rect costRect = new Rect(disc.x, y, disc.width, 12f);
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, new Color(0.7f, 0.9f, 1f)))
                Widgets.Label(costRect, hoveredLeaf.CostHint);
            y += 14f;
        }

        if (hoveredLeaf.ReserveFraction.HasValue) {
            Rect reserveRect = new Rect(disc.x, y, disc.width, 12f);
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.4f)))
                Widgets.Label(reserveRect, $"{hoveredLeaf.ReserveFraction.Value * 100f:F0}%");
            y += 14f;
        }

        if (hoveredLeaf.CooldownTicksRemaining > 0) {
            float seconds = hoveredLeaf.CooldownTicksRemaining / (float)GenTicks.TicksPerRealSecond;
            Rect cdRect = new Rect(disc.x, y, disc.width, 12f);
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, new Color(1f, 0.5f, 0.5f)))
                Widgets.Label(cdRect, "CC_Radial_Cooldown".Translate(seconds.ToString("F1").Named("SECONDS")));
            y += 14f;
        }

        if (hoveredLeaf.IsLocked && hoveredLeaf.LockReason != null) {
            Rect lockRect = new Rect(disc.x, y, disc.width, 12f);
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, new Color(1f, 0.4f, 0.4f)))
                Widgets.Label(lockRect, hoveredLeaf.LockReason);
        }
    }
}
