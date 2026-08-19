using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

/// <summary>
///     The dock's one panel shape. Every boxed surface in every section - metal tiles, detail
///     strips, ability cells, buttons - goes through here, so a radius or hover change lands everywhere at once.
/// </summary>
public static class Panel {
    private static readonly Color HoverWash = new Color(1f, 1f, 1f, 0.045f);

    /// <summary>
    ///     roundTop false squares off the upper corners, which is how a panel joins the one above it -
    ///     the pair reads as a single shape rather than as two boxes that happen to touch.
    /// </summary>
    public static void Draw(Rect rect, Color fill, Color border, bool roundTop = true) {
        Color previous = GUI.color;

        GUI.color = fill;
        Widgets.DrawAtlas(rect, DockTex.RoundFill, roundTop);

        GUI.color = border;
        Widgets.DrawAtlas(rect, DockTex.RoundBorder, roundTop);

        GUI.color = previous;
    }

    /// <summary>
    ///     A panel that runs depth past its own bottom edge into whatever is below, clipped away rather
    ///     than covered - these fills are translucent, so overdrawing would darken the overlap into a visible band.
    /// </summary>
    public static void DrawJoinedDown(
        Rect rect,
        Color fill,
        Color border,
        float depth,
        Color? accent = null,
        float accentWash = 0.18f,
        bool accentRail = true
    ) {
        // The clip bleeds a pixel either side, or a boundary stroke is one rounding error from being trimmed.
        Rect clip = new Rect(rect.x - 1f, rect.y, rect.width + 2f, rect.height + depth);
        Widgets.BeginGroup(clip);

        // Coordinates are relative to the clip; extra height pushes the rounded bottom and its stroke outside it.
        Rect body = new Rect(1f, 0f, rect.width, rect.height + depth + DockTex.Radius);
        Draw(body, fill, border);

        if (accent.HasValue) {
            if (accentRail) Active(body, accent.Value, accentWash);
            else Wash(body, accent.Value, accentWash);
        }

        Widgets.EndGroup();
    }

    /// <summary>
    ///     A panel whose top edge is open across one span and closed elsewhere - what a detail panel
    ///     joined to one tile of a row needs. DrawAtlas can only open the whole top or none, so it draws once per span through a clip.
    /// </summary>
    public static void DrawNotchedTop(
        Rect rect,
        Color fill,
        Color border,
        float notchStart,
        float notchEnd,
        Color? accent = null,
        float accentWash = 0.18f,
        bool accentRail = true
    ) {
        // Snapped to whole pixels - DrawAtlas's own rounding can push the far stroke past a fractional clip edge.
        Rect body = new Rect(
            Mathf.Round(rect.x),
            Mathf.Round(rect.y),
            Mathf.Round(rect.width),
            Mathf.Round(rect.height)
        );

        float start = Mathf.Clamp(Mathf.Round(notchStart), body.x, body.xMax);
        float end = Mathf.Clamp(Mathf.Round(notchEnd), start, body.xMax);

        // Outer spans bleed a pixel past the sides - only the notch boundaries need exact cutting.
        DrawSpan(body, fill, border, body.x - 1f, start, true, accent, accentWash, accentRail);
        DrawSpan(body, fill, border, start, end, false, accent, accentWash, accentRail);
        DrawSpan(body, fill, border, end, body.xMax + 1f, true, accent, accentWash, accentRail);
    }

    /// <summary>
    ///     One vertical slice of a panel, drawn from the full panel's geometry so corners and strokes
    ///     land where they would have unclipped.
    /// </summary>
    private static void DrawSpan(
        Rect body,
        Color fill,
        Color border,
        float from,
        float to,
        bool roundTop,
        Color? accent,
        float accentWash,
        bool accentRail
    ) {
        if (to - from <= 0f) return;

        Rect clip = new Rect(from, body.y, to - from, body.height);
        Widgets.BeginGroup(clip);

        Rect local = new Rect(body.x - clip.x, 0f, body.width, body.height);
        Draw(local, fill, border, roundTop);

        if (accent.HasValue) {
            if (accentRail) Active(local, accent.Value, accentWash, roundTop, 0f);
            else Wash(local, accent.Value, accentWash, roundTop);
        }

        Widgets.EndGroup();
    }

    /// <summary>
    ///     Vanilla's DrawHighlightIfMouseover lays a flat rectangle over rounded panels, overhanging the
    ///     corners; this goes through the same atlas instead. joinDepth mirrors DrawJoinedDown, so a merged tile lights up through the seam too.
    /// </summary>
    public static void Hover(Rect rect, Color border, float joinDepth = 0f) {
        if (!Mouse.IsOver(rect)) return;

        if (joinDepth <= 0f) {
            HoverBody(rect, border);
            return;
        }

        Rect clip = new Rect(rect.x - 1f, rect.y, rect.width + 2f, rect.height + joinDepth);
        Widgets.BeginGroup(clip);
        HoverBody(new Rect(1f, 0f, rect.width, clip.height + DockTex.Radius), border);
        Widgets.EndGroup();
    }

    private static void HoverBody(Rect rect, Color border) {
        Color previous = GUI.color;

        GUI.color = HoverWash;
        Widgets.DrawAtlas(rect, DockTex.RoundFill);

        GUI.color = border;
        Widgets.DrawAtlas(rect, DockTex.RoundBorder);

        GUI.color = previous;
    }

    /// <summary>
    ///     The accent tint on its own. Enough to mark a panel as the open one where "open" is the whole
    ///     story; a surface with a live state to report wants Active instead.
    /// </summary>
    public static void Wash(Rect rect, Color accent, float wash, bool roundTop = true) {
        Color previous = GUI.color;

        GUI.color = new Color(accent.r, accent.g, accent.b, wash);
        Widgets.DrawAtlas(rect, DockTex.RoundFill, roundTop);
        GUI.color = previous;
    }

    /// <summary>
    ///     The tint plus an edge rail. The rail is a state readout, not decoration - it says a metal is
    ///     burning rather than merely selected. Insets open up so a joined panel can run its rail through the seam.
    /// </summary>
    public static void Active(
        Rect rect,
        Color accent,
        float wash = 0.18f,
        bool roundTop = true,
        float railTop = 4f,
        float railBottom = 4f
    ) {
        Wash(rect, accent, wash, roundTop);

        Widgets.DrawBoxSolid(
            new Rect(rect.x + 2f, rect.y + railTop, 3f, rect.height - railTop - railBottom),
            accent
        );
    }

    /// <summary>
    ///     Flaring pulses because it is a burst the player chose and will want to notice ending; a
    ///     steady burn just stays lit. Shared so a tile and the panel it opens breathe on the same beat.
    /// </summary>
    public static float LitWash(bool pulsing) {
        return pulsing ? 0.26f + Mathf.Sin(Time.realtimeSinceStartup * 6f) * 0.08f : 0.18f;
    }
}
