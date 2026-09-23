using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

/// <summary>
///     Every mark in the dock draws white, never tinted - the source art is monochrome line
///     work, and any darker tint costs legibility the title already provides in words.
/// </summary>
public readonly record struct CrestPalette(
    Color Title,
    Color Subtitle,
    Color Rank
);

/// <summary>
///     The identity band atop an expanded section - the accordion header names the system,
///     the crest names the pawn. A label only: nothing here is clickable.
/// </summary>
public static class Crest {
    private const float Gap = 6f;
    private const float MarkSize = 16f;
    private const float MarkGap = 3f;
    private const float MarkRowGap = 5f;

    /// <summary>
    ///     The headline mark is taller than the text beside it, so the crest is as tall as
    ///     whichever is larger - sizing off the text alone let the glyph spill into the table below.
    /// </summary>
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

    /// <summary>
    ///     A pawn with one identity gets one mark beside the title. One assembled out of several
    ///     gets one mark each on their own row - a dozen will not fit in the space one sigil occupies.
    /// </summary>
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

        // Both column and mark hang off the same band, so neither is pinned to the top while the other is centred.
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

        // Measured rather than reserved - a flat width would starve whichever title runs longest.
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

    /// <summary>
    ///     Shrinks marks past capacity rather than dropping any - a mark not drawn is a metal the player
    ///     does not know they have.
    /// </summary>
    private static void DrawMarkRow(Rect row, IReadOnlyList<Texture2D?> marks) {
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
