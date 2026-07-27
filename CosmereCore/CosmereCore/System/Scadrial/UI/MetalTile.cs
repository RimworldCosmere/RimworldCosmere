using Cosmere.Core.UI;
using Cosmere.Core.UI.Dock;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

// How a metal's tile reads at a glance. Inert is not the same as Empty: an
// inert metal has no metalmind to work with, while an empty one is ready to fill.
public enum MetalTileState {
    Idle,
    Active,
    Flaring,
    Inert,
}

// A single cell of the Metallic Arts table: glyph, name, a right-aligned note,
// and the metal's own colour as a level band along the base.
public static class MetalTile {
    public const float Height = 31f;
    private const float GlyphSize = 14f;
    private const float BandHeight = 4f;
    private const float CompoundedBandHeight = 3f;
    private const float BandGap = 2f;

    public static void Draw(
        Rect rect,
        Texture2D? icon,
        string label,
        string note,
        float fraction,
        Color metalColor,
        MetalTileState state,
        Color activeTint,
        float compoundedFraction = 0f,
        Color? compoundedTint = null
    ) {
        bool inert = state == MetalTileState.Inert;
        bool hot = state == MetalTileState.Active || state == MetalTileState.Flaring;

        // Translucent so the parchment behind still shows its grain through the
        // tile, rather than every cell reading as a flat chip laid on top.
        Widgets.DrawBoxSolid(rect, inert
            ? new Color(0.043f, 0.035f, 0.027f, 0.55f)
            : new Color(0.063f, 0.051f, 0.039f, 0.38f));
        Widgets.DrawBoxSolidWithOutline(
            rect.ContractedBy(1f),
            Color.clear,
            hot ? activeTint : new Color(0.298f, 0.243f, 0.169f)
        );

        if (hot) {
            // Flaring pulses because it is a burst the player chose and will want
            // to notice ending; a steady burn just stays lit.
            float wash = state == MetalTileState.Flaring
                ? 0.26f + Mathf.Sin(Time.realtimeSinceStartup * 6f) * 0.08f
                : 0.18f;
            Widgets.DrawBoxSolid(rect, new Color(activeTint.r, activeTint.g, activeTint.b, wash));
            Widgets.DrawBoxSolid(new Rect(rect.x + 1f, rect.y + 2f, 3f, rect.height - 4f), activeTint);
        }

        float tinyH = Text.LineHeightOf(GameFont.Tiny);

        // Empty sits between inert and stocked: there is something to work with
        // here, just nothing in it yet, so it dims without going dead.
        bool empty = !inert && fraction <= 0f && !hot;

        Rect iconRect = new Rect(rect.x + 6f, rect.y + 3f, GlyphSize, GlyphSize);
        if (icon != null) {
            Color prev = GUI.color;
            GUI.color = inert
                ? new Color(metalColor.r, metalColor.g, metalColor.b, 0.35f)
                : empty
                    ? new Color(metalColor.r, metalColor.g, metalColor.b, 0.55f)
                    : metalColor;
            GUI.DrawTexture(iconRect, icon);
            GUI.color = prev;
        }

        // Measured rather than reserved: the axis mark is two characters where a
        // capacity reading is five, and a flat reservation starves the name.
        float noteWidth;
        using (new TextBlock(GameFont.Tiny)) {
            noteWidth = note.NullOrEmpty() ? 0f : Text.CalcSize(note).x + 4f;
        }

        Rect noteRect = new Rect(rect.xMax - 5f - noteWidth, rect.y + 2f, noteWidth, tinyH);
        UIText.EllipsisLabel(noteRect, note, GameFont.Tiny, TextAnchor.MiddleRight, new Color(0.365f, 0.337f, 0.290f));

        Rect nameRect = new Rect(iconRect.xMax + 5f, rect.y + 2f, noteRect.x - iconRect.xMax - 7f, tinyH);
        Color nameColor = inert
            ? new Color(0.435f, 0.412f, 0.373f)
            : hot
                ? new Color(0.949f, 0.965f, 0.980f)
                : empty
                    ? new Color(0.678f, 0.651f, 0.600f)
                    : new Color(0.769f, 0.737f, 0.675f);
        UIText.EllipsisLabel(nameRect, label, GameFont.Tiny, TextAnchor.MiddleLeft, nameColor);

        // Two pools mean two gauges. The stored one stays the primary bar; the
        // compounded one sits below as a thinner stripe on a warmer track, so which
        // is which reads without a legend, and an empty one still shows the pool
        // exists.
        bool twoPools = compoundedTint.HasValue;
        float bottom = rect.yMax - 1f;
        float stack = twoPools ? BandHeight + BandGap + CompoundedBandHeight : BandHeight;

        Rect band = new Rect(rect.x + 4f, bottom - stack, rect.width - 8f, BandHeight);
        DrawGauge(band, inert ? 0f : fraction, metalColor, new Color(0.047f, 0.043f, 0.035f));

        if (!twoPools) return;

        Rect compoundedBand = new Rect(band.x, band.yMax + BandGap, band.width, CompoundedBandHeight);
        DrawGauge(
            compoundedBand,
            inert ? 0f : compoundedFraction,
            compoundedTint!.Value,
            new Color(0.286f, 0.216f, 0.098f)
        );

        if (!inert) Widgets.DrawHighlightIfMouseover(rect);
    }

    private static void DrawGauge(Rect rect, float fraction, Color fill, Color track) {
        Widgets.DrawBoxSolid(rect, track);
        if (fraction <= 0f) return;

        Widgets.DrawBoxSolid(
            new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(fraction), rect.height),
            fill
        );
    }
}
