using Cosmere.Core.UI;
using Cosmere.Core.UI.Dock;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

/// How a metal's tile reads at a glance. Inert is not the same as Empty: an inert metal has no
/// metalmind to work with; an empty one is ready to fill.
public enum MetalTileState {
    Idle,
    Active,
    Flaring,
    Inert,
}

/// A single cell of the table: glyph, name, a right-aligned note, and a colour band along the base.
public static class MetalTile {
    /// Tall enough to hold the glyph and the band without colliding. The band is the reserve readout,
    /// sized to be read rather than a hairline, and sits clear of the bottom edge.
    public const float Height = 64f;

    public static readonly Color Border = new Color(0.298f, 0.243f, 0.169f);

    private static readonly Color InertFill = new Color(0.043f, 0.035f, 0.027f, 0.55f);

    private const float GlyphSize = 36f;
    private const float BandHeight = 12f;
    private const float BandLift = 8f;
    private const float CompoundedBandHeight = 8f;
    private const float BandGap = 3f;

    public static void Draw(
        Rect rect,
        Texture2D? icon,
        string label,
        string note,
        float fraction,
        Color metalColor,
        MetalTileState state,
        float compoundedFraction = 0f,
        Color? compoundedTint = null,
        int savantStage = 0,
        bool joinedBelow = false,
        string? foldLabel = null
    ) {
        bool inert = state == MetalTileState.Inert;
        bool hot = state == MetalTileState.Active || state == MetalTileState.Flaring;

        Color fill = inert ? InertFill : DockPalette.StripFill;

        // burning reads in the name colour, the reserve band and the pinned strip - no accent border, no wash.
        if (joinedBelow) {
            Panel.DrawJoinedDown(rect, fill, Border, MetallicArtsTable.JoinGap);
        } else {
            Panel.Draw(rect, fill, Border);
        }

        DrawSavantFold(rect, savantStage);

        float tinyH = Text.LineHeightOf(GameFont.Tiny);

        // empty sits between inert and stocked - dims without going dead, since there is something to fill.
        bool empty = !inert && fraction <= 0f && !hot;

        // glyph is white, not the metal's colour - copper/bronze are dark browns, unreadable on a dark tile.
        Rect iconRect = new Rect(rect.x + 6f, rect.y + 3f, GlyphSize, GlyphSize);
        if (icon != null) {
            Color prev = GUI.color;
            GUI.color = inert
                ? new Color(1f, 1f, 1f, 0.35f)
                : empty
                    ? new Color(1f, 1f, 1f, 0.55f)
                    : Color.white;
            GUI.DrawTexture(iconRect, icon);
            GUI.color = prev;
        }

        // measured, not reserved - axis mark is 2 chars, capacity reading is 5; a flat reservation starves the name.
        float noteWidth;
        float foldWidth = 0f;
        using (new TextBlock(GameFont.Tiny)) {
            noteWidth = note.NullOrEmpty() ? 0f : Text.CalcSize(note).x + 4f;
            if (!foldLabel.NullOrEmpty()) foldWidth = Text.CalcSize(foldLabel).x + 8f;
        }

        // centred on the glyph, not top-pinned - Tiny text is shorter, so top-align floated the name above the icon.
        float textY = iconRect.y + (GlyphSize - tinyH) / 2f;

        // fold word takes the right edge, note steps left - a second gauge leaves no row below for it.
        if (foldWidth > 0f) {
            Rect foldRect = new Rect(rect.xMax - 5f - foldWidth, textY, foldWidth, tinyH);
            UIText.EllipsisLabel(foldRect, foldLabel!, GameFont.Tiny, TextAnchor.MiddleRight, DockPalette.GroupLabel);
        }

        Rect noteRect = new Rect(rect.xMax - 5f - foldWidth - noteWidth, textY, noteWidth, tinyH);
        UIText.EllipsisLabel(noteRect, note, GameFont.Tiny, TextAnchor.MiddleRight, new Color(0.365f, 0.337f, 0.290f));

        Rect nameRect = new Rect(iconRect.xMax + 5f, textY, noteRect.x - iconRect.xMax - 7f, tinyH);
        Color nameColor = inert
            ? new Color(0.435f, 0.412f, 0.373f)
            : hot
                ? new Color(0.949f, 0.965f, 0.980f)
                : empty
                    ? new Color(0.678f, 0.651f, 0.600f)
                    : new Color(0.769f, 0.737f, 0.675f);
        UIText.EllipsisLabel(nameRect, label, GameFont.Tiny, TextAnchor.MiddleLeft, nameColor);

        if (!inert) Panel.Hover(rect, Border, joinedBelow ? MetallicArtsTable.JoinGap : 0f);

        bool twoPools = compoundedTint.HasValue;
        float bandTop = rect.yMax - BandLift
            - (twoPools ? BandHeight + BandGap + CompoundedBandHeight : BandHeight);

        // skip the band here - the panel below repeats the reading. Height stays fixed either way, or the row jumps.
        if (joinedBelow) return;

        // two pools, two gauges - compounded sits below as a thinner stripe on a warmer track, no legend needed.
        Rect band = new Rect(rect.x + 4f, bandTop, rect.width - 8f, BandHeight);
        DrawGauge(band, inert ? 0f : fraction, metalColor, new Color(0.047f, 0.043f, 0.035f));

        if (!twoPools) return;

        Rect compoundedBand = new Rect(band.x, band.yMax + BandGap, band.width, CompoundedBandHeight);
        DrawGauge(
            compoundedBand,
            inert ? 0f : compoundedFraction,
            compoundedTint!.Value,
            new Color(0.286f, 0.216f, 0.098f)
        );
    }

    /// Savant standing is earned per metal, marked per tile rather than summarised elsewhere. A folded
    /// corner costs no layout, and the three stages read as three brightnesses of the same fold.
    private static void DrawSavantFold(Rect rect, int stage) {
        if (stage <= 0) return;

        Color colour = stage switch {
            1 => new Color(0.780f, 0.549f, 0.180f, 0.45f),
            2 => new Color(0.780f, 0.549f, 0.180f),
            _ => new Color(0.910f, 0.784f, 0.467f),
        };

        // IMGUI has no polygon fill, so the triangle is stacked one-pixel rows.
        const int size = 9;
        for (int i = 0; i < size; i++) {
            float width = size - i;
            Widgets.DrawBoxSolid(new Rect(rect.xMax - width - 1f, rect.y + 1f + i, width, 1f), colour);
        }
    }

    private static void DrawGauge(Rect rect, float fraction, Color fill, Color track) {
        Panel.Draw(rect, track, new Color(0.239f, 0.196f, 0.141f));
        if (fraction <= 0f) return;

        Widgets.DrawBoxSolid(
            new Rect(rect.x + 2f, rect.y + 2f, (rect.width - 4f) * Mathf.Clamp01(fraction), rect.height - 4f),
            fill
        );
    }
}
