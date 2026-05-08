using System;
using UnityEngine;

namespace Cosmere.Lightweave.Runtime;

/// <summary>
/// Per-frame slot used by composable window chrome (WindowHeader / WindowFooter)
/// to publish their painted rect back to the host LightweaveWindow.
///
/// LightweaveWindow.DoWindowContents resets the context before Render and reads
/// it after Render returns to drive drag-region detection, close-X anchoring,
/// and resize-grip claiming.
/// </summary>
internal static class LightweaveWindowContext {
    [ThreadStatic]
    private static Rect? headerRect;

    [ThreadStatic]
    private static bool headerDraggable;

    [ThreadStatic]
    private static bool headerOwnsClose;

    [ThreadStatic]
    private static Rect? footerRect;

    [ThreadStatic]
    private static bool footerOwnsResizeGrip;

    [ThreadStatic]
    private static Types.RadiusSpec? requestedHeaderRadius;

    [ThreadStatic]
    private static Types.RadiusSpec? requestedFooterRadius;

    public static void Reset() {
        headerRect = null;
        headerDraggable = false;
        headerOwnsClose = false;
        footerRect = null;
        footerOwnsResizeGrip = false;
        requestedHeaderRadius = null;
        requestedFooterRadius = null;
    }

    public static void PublishHeader(Rect rect, bool draggable, bool ownsClose) {
        headerRect = rect;
        headerDraggable = draggable;
        headerOwnsClose = ownsClose;
    }

    public static void PublishFooter(Rect rect, bool ownsResizeGrip) {
        footerRect = rect;
        footerOwnsResizeGrip = ownsResizeGrip;
    }

    public static void RequestHeaderRadius(Types.RadiusSpec? radius) {
        requestedHeaderRadius = radius;
    }

    public static void RequestFooterRadius(Types.RadiusSpec? radius) {
        requestedFooterRadius = radius;
    }

    public static Rect? HeaderRect => headerRect;

    public static bool HeaderDraggable => headerDraggable;

    public static bool HeaderOwnsClose => headerOwnsClose;

    public static Rect? FooterRect => footerRect;

    public static bool FooterOwnsResizeGrip => footerOwnsResizeGrip;

    public static Types.RadiusSpec? RequestedHeaderRadius => requestedHeaderRadius;

    public static Types.RadiusSpec? RequestedFooterRadius => requestedFooterRadius;
}
