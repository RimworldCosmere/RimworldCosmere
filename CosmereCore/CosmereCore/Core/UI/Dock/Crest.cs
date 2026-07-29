using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

// No glyph tint on purpose: every mark in the dock is drawn white. The source art
// is monochrome line work on dark panels, and any tint darker than white - a section
// accent, an order colour, a metal's own colour - costs legibility for identity the
// title already states in words.
public readonly record struct CrestPalette(
    Color Title,
    Color Subtitle,
    Color Rank
);

// The identity band at the top of an expanded section. The accordion header says
// what the system is; the crest says what the pawn is - Lightweaver rather than
// Surgebinding, Mistborn rather than Allomancy.
//
// A label, not a control. Anything the player can do belongs in a button that says
// so - a clickable crest is a hidden control, and the only tooltip it could
// honestly carry is the title it already shows.
public static class Crest {
    private const float Gap = 6f;
    private const float MarkSize = 16f;
    private const float MarkGap = 3f;
    private const float MarkRowGap = 5f;

    // The headline mark is taller than the two lines of text beside it, so the crest
    // is as tall as whichever is larger. Sizing it off the text alone let the glyph
    // spill into the table below.
    private static float GlyphSizeFor(GameFont titleFont) {
        return titleFont == GameFont.Medium ? 60f : 44f;
    }

    public static float HeightFor(bool hasSubtitle, bool hasMarkRow = false, GameFont titleFont = GameFont.Small) {
        float text = Text.LineHeightOf(titleFont);
        if (hasSubtitle) text += Text.LineHeightOf(GameFont.Tiny);

        float height = Mathf.Max(text, GlyphSizeFor(titleFont));
        if (hasMarkRow) height += MarkSize + MarkRowGap;

        return height;
    }

    // A pawn with one identity gets one mark beside the title. A pawn assembled out
    // of several gets one mark each, on their own row - there is no honest way to
    // squeeze a dozen into the space one sigil occupies.
    public static void Draw(
        Rect rect,
        Texture2D? glyph,
        string title,
        string? subtitle,
        string? rank,
        CrestPalette palette,
        GameFont titleFont = GameFont.Small,
        IReadOnlyList<Texture2D?>? marks = null
    ) {
        float titleHeight = Text.LineHeightOf(titleFont);
        float glyphSize = GlyphSizeFor(titleFont);

        float textHeight = titleHeight + (subtitle.NullOrEmpty() ? 0f : Text.LineHeightOf(GameFont.Tiny));
        float band = Mathf.Max(textHeight, glyphSize);

        // Both column and mark hang off the same band, so neither is pinned to the top
        // while the other is centred.
        float textTop = rect.y + (band - textHeight) / 2f;

        float textX = rect.x;
        if (glyph != null) {
            Rect glyphRect = new Rect(rect.x, rect.y + (band - glyphSize) / 2f, glyphSize, glyphSize);
            Color previous = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(glyphRect, glyph);
            GUI.color = previous;
            textX = glyphRect.xMax + Gap;
        }

        // Measured rather than reserved: "3rd Ideal" and "17 metals" are different
        // widths, and a flat reservation starves whichever title runs longest.
        float rankWidth = 0f;
        if (!rank.NullOrEmpty()) {
            using (new TextBlock(GameFont.Tiny)) {
                rankWidth = Text.CalcSize(rank).x + Gap;
            }
        }

        UIText.EllipsisLabel(
            new Rect(textX, textTop, rect.xMax - textX - rankWidth, titleHeight),
            title,
            titleFont,
            TextAnchor.MiddleLeft,
            palette.Title
        );

        if (!rank.NullOrEmpty()) {
            UIText.EllipsisLabel(
                new Rect(rect.xMax - rankWidth, textTop, rankWidth, titleHeight),
                rank!,
                GameFont.Tiny,
                TextAnchor.MiddleRight,
                palette.Rank
            );
        }

        float y = textTop + titleHeight;
        if (!subtitle.NullOrEmpty()) {
            float subtitleHeight = Text.LineHeightOf(GameFont.Tiny);
            UIText.EllipsisLabel(
                new Rect(textX, y, rect.xMax - textX, subtitleHeight),
                subtitle!,
                GameFont.Tiny,
                TextAnchor.MiddleLeft,
                palette.Subtitle
            );
            y += subtitleHeight;
        }

        if (marks == null || marks.Count == 0) return;

        DrawMarkRow(new Rect(rect.x, y + MarkRowGap, rect.width, MarkSize), marks);
    }

    private static void DrawMarkRow(Rect row, IReadOnlyList<Texture2D?> marks) {
        // Sixteen marks fit the narrowest body at this size. Past that the row would
        // run off, so it shrinks them rather than dropping any - a mark that is not
        // drawn is a metal the player does not know they have.
        float size = Mathf.Min(MarkSize, (row.width - MarkGap * (marks.Count - 1)) / marks.Count);
        float x = row.x;

        Color previous = GUI.color;
        GUI.color = Color.white;
        for (int i = 0; i < marks.Count; i++) {
            if (marks[i] != null) {
                GUI.DrawTexture(new Rect(x, row.y + (MarkSize - size) / 2f, size, size), marks[i]);
            }

            x += size + MarkGap;
        }

        GUI.color = previous;
    }
}
