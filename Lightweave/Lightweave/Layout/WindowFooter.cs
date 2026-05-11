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
        Action<List<LightweaveNode>>? children = null,
        bool drawDivider = true,
        bool showResizeGrip = false,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);

        LightweaveNode node = NodeBuilder.New("WindowFooter", line, file);
        node.ApplyStyling("window-footer", style, classes, id);

        Style s0 = node.GetResolvedStyle();
        float footerH = s0.Height is { Mode: Length.Kind.Rem } hh
            ? hh.ToPixels(0f, 0f)
            : new Rem(4f).ToPixels();
        node.PreferredHeight = footerH;
        node.Children.AddRange(kids);

        EdgeInsets defaultPad = new EdgeInsets(
            Top: SpacingScale.Md,
            Bottom: SpacingScale.Md,
            Left: SpacingScale.Md,
            Right: SpacingScale.Xs
        );

        node.Paint = (rect, paintChildren) => {
            Style s = node.GetResolvedStyle();
            EdgeInsets pad = s.Padding ?? defaultPad;
            BackgroundSpec bg = s.Background ?? BackgroundSpec.Of(ThemeSlot.SurfaceRaised);

            LightweaveWindowContext.PublishFooter(rect, showResizeGrip);

            RadiusSpec? effectiveRadius = s.Radius ?? LightweaveWindowContext.RequestedFooterRadius;
            PaintBox.Draw(rect, bg, null, effectiveRadius);

            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            if (drawDivider) {
                Color borderColor;
                BorderSpec? sb = s.Border;
                if (sb.HasValue && sb.Value.Color != null) {
                    borderColor = sb.Value.Color switch {
                        ColorRef.Literal lit => lit.Value,
                        ColorRef.Token tok => theme.GetColor(tok.Slot),
                        _ => theme.GetColor(ThemeSlot.BorderDefault),
                    };
                }
                else {
                    borderColor = theme.GetColor(ThemeSlot.BorderDefault);
                }
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
