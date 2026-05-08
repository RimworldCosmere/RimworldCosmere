using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Theme;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Layout;

[Doc(
    Id = "windowfooter",
    Summary = "Footer region of a LightweaveWindow with theme fill, optional top divider, and resize grip.",
    WhenToUse = "Pin status text, dialog button rows, or a resize grip to the bottom of a window.",
    SourcePath = "Lightweave/Lightweave/Layout/WindowFooter.cs"
)]
public static class WindowFooter {
    public static LightweaveNode Create(
        [DocParam("Footer children appended inside the band. Compose with HStack for multi-column layouts.")]
        Action<List<LightweaveNode>>? children = null,
        [DocParam("Footer band height.", TypeOverride = "Rem", DefaultOverride = "4rem")]
        Rem? height = null,
        [DocParam("Theme slot used for the footer background fill.")]
        ThemeSlot backgroundSlot = ThemeSlot.SurfaceRaised,
        [DocParam("Inner padding around footer children.", TypeOverride = "EdgeInsets?", DefaultOverride = "1rem")]
        EdgeInsets? padding = null,
        [DocParam("Draw a 1px divider above the footer band.")]
        bool drawDivider = true,
        [DocParam("Theme slot for the divider color.")]
        ThemeSlot dividerSlot = ThemeSlot.BorderDefault,
        [DocParam("Render a diagonal resize grip in the trailing-bottom corner.")]
        bool showResizeGrip = false,
        [DocParam("Override corner radius for the footer background. When null, falls back to the host window's requested bottom-rounding.")]
        RadiusSpec? cornerRadius = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Rem h = height ?? new Rem(4f);
        float footerH = h.ToPixels();
        EdgeInsets pad = padding ?? new EdgeInsets(
            Top: SpacingScale.Md,
            Bottom: SpacingScale.Md,
            Left: SpacingScale.Md,
            Right: SpacingScale.Xs
        );

        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);

        LightweaveNode node = NodeBuilder.New("WindowFooter", line, file);
        node.PreferredHeight = footerH;
        node.Children.AddRange(kids);

        node.Paint = (rect, paintChildren) => {
            LightweaveWindowContext.PublishFooter(rect, showResizeGrip);

            RadiusSpec? effectiveRadius = cornerRadius ?? LightweaveWindowContext.RequestedFooterRadius;
            PaintBox.Draw(rect, BackgroundSpec.Of(backgroundSlot), null, effectiveRadius);

            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            if (drawDivider) {
                Color borderColor = theme.GetColor(dividerSlot);
                Rect divider = new Rect(rect.x, rect.y, rect.width, 1f);
                Color saved = GUI.color;
                GUI.color = borderColor;
                GUI.DrawTexture(RectSnap.Snap(divider), BaseContent.WhiteTex);
                GUI.color = saved;
            }

            (float left, float top, float right, float bottom) = pad.Resolve(dir);
            Rect content = new Rect(
                rect.x + left,
                rect.y + top + (drawDivider ? 1f : 0f),
                Mathf.Max(0f, rect.width - left - right),
                Mathf.Max(0f, rect.height - top - bottom - (drawDivider ? 1f : 0f))
            );

            int count = kids.Count;
            if (count > 0) {
                for (int i = 0; i < count; i++) {
                    kids[i].MeasuredRect = content;
                }

                paintChildren();
            }

            if (showResizeGrip) {
                DrawResizeGrip(rect, theme, rtl);
            }
        };
        return node;
    }

    private static void DrawResizeGrip(Rect footerRect, Theme.Theme theme, bool rtl) {
        const float pad = 4f;
        const float dot = 2f;
        const float gap = 2f;
        float startX = rtl ? footerRect.x + pad : footerRect.xMax - pad - (dot + gap) * 3f + gap;
        float baseY = footerRect.yMax - pad - dot;

        Color hint = theme.GetColor(ThemeSlot.TextMuted);
        Color saved = GUI.color;
        GUI.color = hint;

        for (int row = 0; row < 3; row++) {
            for (int col = row; col < 3; col++) {
                float x = startX + col * (dot + gap);
                float y = baseY - row * (dot + gap);
                Rect d = new Rect(x, y, dot, dot);
                GUI.DrawTexture(RectSnap.Snap(d), BaseContent.WhiteTex);
            }
        }

        GUI.color = saved;
    }
}
