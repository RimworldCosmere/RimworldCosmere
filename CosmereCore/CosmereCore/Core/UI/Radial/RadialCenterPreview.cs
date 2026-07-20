using System;
using UnityEngine;
using Verse;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Dock;

namespace Cosmere.Core.UI.Radial;

public static class RadialCenterPreview {
    public static void Draw(Vector2 center, RadialLeaf? hoveredLeaf, string breadcrumb, bool browseMode, Action back, Action close) {
        float r = RadialLayout.CenterRadius;
        Rect disc = new Rect(center.x - r, center.y - r, r * 2f, r * 2f);
        Widgets.DrawBoxSolid(disc, new Color(0.078f, 0.098f, 0.125f, 0.96f));
        Widgets.DrawBoxSolidWithOutline(disc, Color.clear, DockPalette.Border);

        Rect crumbRect = new Rect(disc.x + 8f, disc.y + 10f, disc.width - 16f, 12f);
        UIText.EllipsisLabel(crumbRect, breadcrumb, GameFont.Tiny, TextAnchor.MiddleCenter, DockPalette.GroupLabel);

        float y = crumbRect.yMax + 4f;
        if (hoveredLeaf == null) {
            Rect promptRect = new Rect(disc.x + 10f, y, disc.width - 20f, 28f);
            UIText.EllipsisLabel(promptRect, "CC_Radial_Hover_Prompt".Translate(), GameFont.Tiny, TextAnchor.MiddleCenter, DockPalette.MutedText);
        }
        else {
            Rect titleRect = new Rect(disc.x + 8f, y, disc.width - 16f, 18f);
            UIText.EllipsisLabel(titleRect, hoveredLeaf.Label, GameFont.Small, TextAnchor.MiddleCenter, new Color(0.75f, 0.89f, 0.95f));
            y = titleRect.yMax + 2f;

            if (!hoveredLeaf.Description.NullOrEmpty()) {
                Rect descRect = new Rect(disc.x + 14f, y, disc.width - 28f, 26f);
                string desc = hoveredLeaf.Description!.Truncate(descRect.width * 2f);
                using (new TextBlock(GameFont.Tiny, TextAnchor.UpperCenter, DockPalette.MutedText)) {
                    Widgets.Label(descRect, desc);
                }

                TooltipHandler.TipRegion(descRect, hoveredLeaf.Description);
                y = descRect.yMax + 2f;
            }

            string meta = BuildMetaLine(hoveredLeaf);
            if (meta.Length > 0) {
                Rect metaRect = new Rect(disc.x + 8f, y, disc.width - 16f, 12f);
                UIText.EllipsisLabel(metaRect, meta, GameFont.Tiny, TextAnchor.MiddleCenter, DockPalette.HotLabel);
                y = metaRect.yMax + 3f;
            }

            if (hoveredLeaf.ReserveFraction.HasValue) {
                Rect barRect = new Rect(disc.x + 26f, y, disc.width - 52f, 6f);
                Widgets.DrawBoxSolid(barRect, new Color(0f, 0f, 0f, 0.6f));
                Widgets.DrawBoxSolid(
                    new Rect(barRect.x + 1f, barRect.y + 1f, (barRect.width - 2f) * Mathf.Clamp01(hoveredLeaf.ReserveFraction.Value), 4f),
                    new Color(0.478f, 0.784f, 0.902f)
                );
                y = barRect.yMax + 3f;
            }

            if (hoveredLeaf.IsLocked && hoveredLeaf.LockReason != null) {
                Rect lockRect = new Rect(disc.x + 10f, y, disc.width - 20f, 12f);
                UIText.EllipsisLabel(lockRect, hoveredLeaf.LockReason, GameFont.Tiny, TextAnchor.MiddleCenter, DockPalette.Flare);
                y = lockRect.yMax;
            }
        }

        if (browseMode) {
            float btnW = 56f;
            Rect backRect = new Rect(center.x - btnW - 4f, disc.yMax - 30f, btnW, 20f);
            Rect closeRect = new Rect(center.x + 4f, disc.yMax - 30f, btnW, 20f);
            if (Widgets.ButtonText(backRect, "CC_Radial_Back".Translate())) back();
            if (Widgets.ButtonText(closeRect, "CC_Radial_Close".Translate())) close();
        }
        else {
            Rect hintRect = new Rect(disc.x + 8f, disc.yMax - 24f, disc.width - 16f, 12f);
            UIText.EllipsisLabel(hintRect, "CC_Radial_Hint_Release".Translate(), GameFont.Tiny, TextAnchor.MiddleCenter, DockPalette.GroupLabel);
        }
    }

    private static string BuildMetaLine(RadialLeaf leaf) {
        string meta = "";
        if (leaf.CostHint != null) meta = leaf.CostHint;
        if (leaf.CooldownTicksRemaining > 0) {
            float seconds = leaf.CooldownTicksRemaining / (float)GenTicks.TicksPerRealSecond;
            string cd = "CC_Radial_Cooldown".Translate(seconds.ToString("F1").Named("SECONDS"));
            meta = meta.Length > 0 ? meta + " - " + cd : cd;
        }

        return meta;
    }
}