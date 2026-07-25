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

        UIText.EllipsisLabel(ChordRow(center, -88f, tinyH), breadcrumb, GameFont.Tiny, TextAnchor.MiddleCenter, DockPalette.GroupLabel);

        if (hoveredLeaf == null) {
            if (hoveredTitle != null) {
                UIText.EllipsisLabel(
                    ChordRow(center, -28f, smallH),
                    hoveredTitle,
                    GameFont.Small,
                    TextAnchor.MiddleCenter,
                    new Color(0.75f, 0.89f, 0.95f)
                );
                UIText.EllipsisLabel(
                    ChordRow(center, 0f, tinyH),
                    "CC_Radial_Open_Prompt".Translate(),
                    GameFont.Tiny,
                    TextAnchor.MiddleCenter,
                    DockPalette.MutedText
                );
            }
            else {
                UIText.EllipsisLabel(
                    ChordRow(center, -9f, tinyH),
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
            const float buttonSize = 30f;
            const float buttonGap = 10f;
            float buttonY = center.y + 50f;
            Rect backRect = new Rect(center.x - buttonSize - buttonGap / 2f, buttonY, buttonSize, buttonSize);
            Rect closeRect = new Rect(center.x + buttonGap / 2f, buttonY, buttonSize, buttonSize);

            DrawIconButton(backRect, TexButton.Reveal, "CC_Radial_Back".Translate(), 0.55f, true, back);
            DrawIconButton(closeRect, TexButton.CloseXSmall, "CC_Radial_Close".Translate(), 0.55f, false, close);
        }
        else {
            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float hintY = 56f;
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

    private static void DrawIconButton(Rect rect, Texture2D icon, string tooltip, float iconScale, bool mirrored, Action onClick) {
        bool hovered = Mouse.IsOver(rect);
        Widgets.DrawBoxSolid(rect, new Color(1f, 1f, 1f, hovered ? 0.12f : 0.05f));

        float targetHeight = rect.height * iconScale;
        float aspect = icon.height > 0 ? icon.width / (float)icon.height : 1f;
        float drawHeight = targetHeight;
        float drawWidth = targetHeight * aspect;
        if (drawWidth > rect.width) {
            drawWidth = rect.width;
            drawHeight = drawWidth / aspect;
        }

        Rect iconRect = new Rect(
            rect.center.x - drawWidth / 2f,
            rect.center.y - drawHeight / 2f,
            drawWidth,
            drawHeight
        );

        Color prev = GUI.color;
        GUI.color = hovered ? Color.white : new Color(0.78f, 0.80f, 0.82f);
        if (mirrored) {
            Matrix4x4 prevMatrix = GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(-1f, 1f), iconRect.center);
            GUI.DrawTexture(iconRect, icon);
            GUI.matrix = prevMatrix;
        }
        else {
            GUI.DrawTexture(iconRect, icon);
        }

        GUI.color = prev;

        TooltipHandler.TipRegion(rect, tooltip);
        Verse.Sound.MouseoverSounds.DoRegion(rect);
        if (Widgets.ButtonInvisible(rect)) onClick();
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
