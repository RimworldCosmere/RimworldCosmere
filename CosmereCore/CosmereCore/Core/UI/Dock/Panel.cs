using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

// The dock's one panel shape. Every boxed surface in every section - metal tiles,
// detail strips, ability cells, buttons - goes through here, so a change to the
// radius or the hover is a change everywhere rather than a change in five files.
public static class Panel {
    private static readonly Color HoverWash = new Color(1f, 1f, 1f, 0.045f);

    // roundTop false squares off the upper corners, which is how a panel joins the
    // one above it: the pair reads as a single shape rather than as two boxes that
    // happen to touch.
    public static void Draw(Rect rect, Color fill, Color border, bool roundTop = true) {
        Color previous = GUI.color;

        GUI.color = fill;
        Widgets.DrawAtlas(rect, DockTex.RoundFill, roundTop);

        GUI.color = border;
        Widgets.DrawAtlas(rect, DockTex.RoundBorder, roundTop);

        GUI.color = previous;
    }

    // A panel that runs `depth` past its own bottom edge into whatever is drawn
    // below, with that bottom clipped away rather than merely covered. Overdrawing
    // and letting the next panel hide the difference does not work here: these fills
    // are translucent, so the stroke reads straight through and the overlapping
    // fills darken into a visible band. Clipping leaves nothing to show through.
    public static void DrawJoinedDown(
        Rect rect,
        Color fill,
        Color border,
        float depth,
        Color? accent = null,
        float accentWash = 0.18f,
        bool accentRail = true
    ) {
        // Only the bottom is being cut, so the clip bleeds a pixel either side rather
        // than sitting exactly on the panel's own edges - a stroke landing on the clip
        // boundary is one rounding error away from being trimmed off entirely.
        Rect clip = new Rect(rect.x - 1f, rect.y, rect.width + 2f, rect.height + depth);
        Widgets.BeginGroup(clip);

        // One corner taller than the clip, so the rounded bottom and its stroke land
        // outside it. Group coordinates are relative to the clip, hence the offset.
        Rect body = new Rect(1f, 0f, rect.width, rect.height + depth + DockTex.Radius);
        Draw(body, fill, border);

        if (accent.HasValue) {
            if (accentRail) Active(body, accent.Value, accentWash);
            else Wash(body, accent.Value, accentWash);
        }

        Widgets.EndGroup();
    }

    // A panel whose top edge is open across one span and closed everywhere else.
    // That is what a detail panel joined to a single tile of a two-tile row needs: no
    // stroke where the tile flows down into it, an ordinary rounded top where its
    // neighbour sits clear above it. DrawAtlas can only open the whole top or none of
    // it, so the panel is drawn once per span through a clip.
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
        // Snapped to whole pixels first. DrawAtlas rounds its own rect, and inside a
        // clip sitting at a fractional offset that rounding can push the far stroke one
        // pixel past the clip edge - which is where a vanishing right border comes from.
        Rect body = new Rect(
            Mathf.Round(rect.x),
            Mathf.Round(rect.y),
            Mathf.Round(rect.width),
            Mathf.Round(rect.height)
        );

        float start = Mathf.Clamp(Mathf.Round(notchStart), body.x, body.xMax);
        float end = Mathf.Clamp(Mathf.Round(notchEnd), start, body.xMax);

        // The outer spans bleed a pixel past the panel's own sides. Only the notch
        // boundaries need cutting; letting the outside edges sit exactly on the clip
        // leaves the side strokes one rounding error from being trimmed off.
        DrawSpan(body, fill, border, body.x - 1f, start, true, accent, accentWash, accentRail);
        DrawSpan(body, fill, border, start, end, false, accent, accentWash, accentRail);
        DrawSpan(body, fill, border, end, body.xMax + 1f, true, accent, accentWash, accentRail);
    }

    // One vertical slice of a panel, drawn from the full panel's geometry so corners
    // and strokes land where they would have unclipped.
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

    // A wash plus a lifted edge. Vanilla's DrawHighlightIfMouseover lays a flat grey
    // *rectangle* over the rect, so on a rounded panel it overhangs all four corners
    // and reads as a square patch sitting on top rather than as the panel lighting up.
    // Going through the same atlas keeps the highlight the shape of what it highlights.
    //
    // joinDepth mirrors DrawJoinedDown: a tile merged into the panel below it has to
    // light up through the join too, or hovering it draws a lid across the seam.
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

    // The accent tint on its own. Enough to mark a panel as the open one where "open"
    // is the whole story; a surface that also has a live state to report wants Active.
    public static void Wash(Rect rect, Color accent, float wash, bool roundTop = true) {
        Color previous = GUI.color;

        GUI.color = new Color(accent.r, accent.g, accent.b, wash);
        Widgets.DrawAtlas(rect, DockTex.RoundFill, roundTop);
        GUI.color = previous;
    }

    // The tint plus an edge rail. The rail is a state readout, not decoration - it is
    // what says a metal is burning rather than merely selected - so it belongs on
    // surfaces that have such a state and nowhere else. The insets open up so a panel
    // joined to another can run its rail through the seam rather than stopping short
    // on both sides of it.
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

    // Flaring pulses because it is a burst the player chose and will want to notice
    // ending; a steady burn just stays lit. Shared so a tile and the detail panel it
    // opens breathe on the same beat instead of drifting apart.
    public static float LitWash(bool pulsing) {
        return pulsing ? 0.26f + Mathf.Sin(Time.realtimeSinceStartup * 6f) * 0.08f : 0.18f;
    }
}
