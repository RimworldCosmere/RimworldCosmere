using System;
using UnityEngine;
using Verse;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Dock;

namespace Cosmere.Core.UI.Radial;

public static class RadialCenterPreview {
    public static void Draw(Vector2 center, RadialLeaf? hoveredLeaf, string? hoveredTitle, string breadcrumb, bool browseMode, Action back, Action close) {
        float r = RadialLayout.CenterRadius;
        Rect disc = new Rect(center.x - r, center.y - r, r * 2f, r * 2f);
        Texture2D discTex = RadialWedgeTex.Disc();
        Color prev = GUI.color;
        GUI.color = DockPalette.Border;
        GUI.DrawTexture(disc.ExpandedBy(1.5f), discTex);
        GUI.color = new Color(0.078f, 0.098f, 0.125f, 0.96f);
        GUI.DrawTexture(disc, discTex);
        GUI.color = prev;

        float tinyH = Text.LineHeightOf(GameFont.Tiny);
        float smallH = Text.LineHeightOf(GameFont.Small);

        UIText.EllipsisLabel(ChordRow(center, -96f, tinyH), breadcrumb, GameFont.Tiny, TextAnchor.MiddleCenter, DockPalette.GroupLabel);

        if (hoveredLeaf == null) {
            if (hoveredTitle != null) {
                UIText.EllipsisLabel(
                    ChordRow(center, -34f, smallH),
                    hoveredTitle,
                    GameFont.Small,
                    TextAnchor.MiddleCenter,
                    new Color(0.75f, 0.89f, 0.95f)
                );
                UIText.EllipsisLabel(
                    ChordRow(center, -4f, tinyH),
                    "CC_Radial_Open_Prompt".Translate(),
                    GameFont.Tiny,
                    TextAnchor.MiddleCenter,
                    DockPalette.MutedText
                );
            }
            else {
                UIText.EllipsisLabel(
                    ChordRow(center, -12f, tinyH),
                    "CC_Radial_Hover_Prompt".Translate(),
                    GameFont.Tiny,
                    TextAnchor.MiddleCenter,
                    DockPalette.MutedText
                );
            }
        }
        else {
            bool hasParent = !hoveredTitle.NullOrEmpty() && hoveredTitle != hoveredLeaf.Label;
            if (hasParent) {
                UIText.EllipsisLabel(
                    ChordRow(center, -76f, tinyH),
                    hoveredTitle!,
                    GameFont.Tiny,
                    TextAnchor.MiddleCenter,
                    DockPalette.MutedText
                );
            }

            UIText.EllipsisLabel(
                ChordRow(center, hasParent ? -56f : -60f, smallH),
                hoveredLeaf.Label,
                GameFont.Small,
                TextAnchor.MiddleCenter,
                new Color(0.75f, 0.89f, 0.95f)
            );

            if (!hoveredLeaf.Description.NullOrEmpty()) {
                Rect descRect = ChordRow(center, -30f, tinyH * 2f);
                string desc = hoveredLeaf.Description!.Truncate(descRect.width * 2f);
                using (new TextBlock(GameFont.Tiny, TextAnchor.UpperCenter, DockPalette.MutedText)) {
                    Widgets.Label(descRect, desc);
                }

                TooltipHandler.TipRegion(descRect, hoveredLeaf.Description);
            }

            if (hoveredLeaf.IsLocked && hoveredLeaf.LockReason != null) {
                UIText.EllipsisLabel(ChordRow(center, 8f, tinyH), hoveredLeaf.LockReason, GameFont.Tiny, TextAnchor.MiddleCenter, DockPalette.Flare);
            }
            else {
                string meta = BuildMetaLine(hoveredLeaf);
                if (meta.Length > 0) {
                    UIText.EllipsisLabel(ChordRow(center, 8f, tinyH), meta, GameFont.Tiny, TextAnchor.MiddleCenter, DockPalette.HotLabel);
                }
            }

            if (hoveredLeaf.ReserveFraction.HasValue) {
                float fraction = Mathf.Clamp01(hoveredLeaf.ReserveFraction.Value);
                Rect labelRow = ChordRow(center, 28f, tinyH);
                UIText.EllipsisLabel(
                    labelRow,
                    "CC_Radial_Reserve".Translate(Mathf.RoundToInt(fraction * 100f).Named("PERCENT")),
                    GameFont.Tiny,
                    TextAnchor.MiddleCenter,
                    DockPalette.MutedText
                );

                Rect barRow = ChordRow(center, 48f, 6f);
                Widgets.DrawBoxSolid(barRow, new Color(0f, 0f, 0f, 0.6f));
                Widgets.DrawBoxSolid(
                    new Rect(barRow.x + 1f, barRow.y + 1f, (barRow.width - 2f) * fraction, 4f),
                    new Color(0.478f, 0.784f, 0.902f)
                );
            }
        }

        if (browseMode) {
            const float iconSize = 26f;
            Rect backRect = new Rect(center.x - iconSize - 6f, center.y + 60f, iconSize, iconSize);
            Rect closeRect = new Rect(center.x + 6f, center.y + 60f, iconSize, iconSize);

            TooltipHandler.TipRegion(backRect, "CC_Radial_Back".Translate());
            TooltipHandler.TipRegion(closeRect, "CC_Radial_Close".Translate());

            Matrix4x4 backMatrix = GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(-1f, 1f), backRect.center);
            GUI.DrawTexture(backRect, TexButton.Reveal);
            GUI.matrix = backMatrix;
            if (Widgets.ButtonInvisible(backRect)) back();
            Widgets.DrawHighlightIfMouseover(backRect);
            if (Widgets.ButtonImage(closeRect, TexButton.CloseXSmall, true)) close();
        }
        else {
            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float hintY = 60f;
            UIText.EllipsisLabel(
                ChordRow(center, hintY, tinyH),
                shiftHeld
                    ? "CC_Radial_Hint_FlareArmed".Translate()
                    : hoveredLeaf != null
                        ? "CC_Radial_Hint_ReleaseFlare".Translate()
                        : "CC_Radial_Hint_Release".Translate(),
                GameFont.Tiny,
                TextAnchor.MiddleCenter,
                shiftHeld ? DockPalette.Flare : DockPalette.GroupLabel
            );


        }
    }

    private static Rect ChordRow(Vector2 center, float dyTop, float height) {
        float r = RadialLayout.CenterRadius - 6f;
        float worstDy = Mathf.Max(Mathf.Abs(dyTop), Mathf.Abs(dyTop + height));
        float half = worstDy >= r ? 0f : Mathf.Sqrt(r * r - worstDy * worstDy);
        return new Rect(center.x - half, center.y + dyTop, half * 2f, height);
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
