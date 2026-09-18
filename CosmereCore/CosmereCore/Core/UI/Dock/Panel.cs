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
    public static void DrawJoinedDown(Rect rect, Color fill, Color border, float depth) {
        // The clip bleeds a pixel either side, or a boundary stroke is one rounding error from being trimmed.
        Rect clip = new Rect(rect.x - 1f, rect.y, rect.width + 2f, rect.height + depth);
        Widgets.BeginGroup(clip);

        // Coordinates are relative to the clip; extra height pushes the rounded bottom and its stroke outside it.
        Rect body = new Rect(1f, 0f, rect.width, rect.height + depth + DockTex.Radius);
        Draw(body, fill, border);

        Widgets.EndGroup();
    }

    /// <summary>
    ///     A panel whose top edge is open across one span and closed elsewhere - what a detail panel
    ///     joined to one tile of a row needs. DrawAtlas can only open the whole top or none, so it draws once per span through a clip.
    /// </summary>
    public static void DrawNotchedTop(Rect rect, Color fill, Color border, float notchStart, float notchEnd) {
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
        DrawSpan(body, fill, border, body.x - 1f, start, true);
        DrawSpan(body, fill, border, start, end, false);
        DrawSpan(body, fill, border, end, body.xMax + 1f, true);
    }

    /// <summary>
    ///     One vertical slice of a panel, drawn from the full panel's geometry so corners and strokes
    ///     land where they would have unclipped.
    /// </summary>
    private static void DrawSpan(Rect body, Color fill, Color border, float from, float to, bool roundTop) {
        if (to - from <= 0f) return;

        Rect clip = new Rect(from, body.y, to - from, body.height);
        Widgets.BeginGroup(clip);

        Rect local = new Rect(body.x - clip.x, 0f, body.width, body.height);
        Draw(local, fill, border, roundTop);

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
}
