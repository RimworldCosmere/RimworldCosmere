using Cosmere.Core.UI;
using Cosmere.Core.UI.Dock;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

/// How a metal's tile reads at a glance. Inert is not the same as Empty: an
/// inert metal has no metalmind to work with, while an empty one is ready to fill.
public enum MetalTileState {
    Idle,
    Active,
    Flaring,
    Inert,
}

/// A single cell of the Metallic Arts table: glyph, name, a right-aligned note,
/// and the metal's own colour as a level band along the base.
public static class MetalTile {
    public const float Height = 31f;
    private const float GlyphSize = 14f;
    private const float BandHeight = 5f;

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
        Color compoundedTint = default
    ) {
        bool inert = state == MetalTileState.Inert;
        bool hot = state == MetalTileState.Active || state == MetalTileState.Flaring;

        Widgets.DrawBoxSolid(rect, inert ? new Color(0.078f, 0.071f, 0.063f) : new Color(0.098f, 0.090f, 0.075f));
        Widgets.DrawBoxSolidWithOutline(
            rect.ContractedBy(1f),
            Color.clear,
            hot ? activeTint : new Color(0.204f, 0.180f, 0.149f)
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

        // Inset so a full reserve still reads as a gauge. Edge-to-edge, it just
        // looks like the tile grew a coloured border.
        Rect band = new Rect(rect.x + 4f, rect.yMax - BandHeight - 1f, rect.width - 8f, BandHeight - 1f);
        Widgets.DrawBoxSolid(band, new Color(0.047f, 0.043f, 0.035f));
        if (!inert && fraction > 0f) {
            Widgets.DrawBoxSolid(
                new Rect(band.x, band.y, band.width * Mathf.Clamp01(fraction), band.height),
                metalColor
            );
        }

        // Compounded charge rides at the end of the fill in its own colour, so the
        // split reads off the bar and the number stays a plain total.
        if (inert || compoundedFraction <= 0f) return;

        float compounded = Mathf.Clamp01(compoundedFraction);
        float start = Mathf.Clamp01(fraction - compounded);
        Widgets.DrawBoxSolid(
            new Rect(band.x + band.width * start, band.y, band.width * compounded, band.height),
            compoundedTint == default ? metalColor : compoundedTint
        );

        if (!inert) Widgets.DrawHighlightIfMouseover(rect);
    }
}
