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
using Verse.Sound;

namespace Cosmere.Lightweave.Layout;

[Doc(
    Id = "windowheader",
    Summary = "Title bar for a LightweaveWindow with auto close button and drag publishing.",
    WhenToUse = "Override LightweaveWindow.Header() to give a window a title row with chrome controls.",
    SourcePath = "Lightweave/Lightweave/Layout/WindowHeader.cs"
)]
public static class WindowHeader {
    public static LightweaveNode Create(
        string? title = null,
        bool showClose = true,
        Action? onClose = null,
        bool draggable = true,
        bool drawDivider = true,
        CloseButtonVariant closeStyle = CloseButtonVariant.Default,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("WindowHeader", line, file);
        node.ApplyStyling("window-header", style, classes, id);

        Style s0 = node.GetResolvedStyle();
        float headerH = s0.Height is { Mode: Length.Kind.Rem } hh
            ? hh.ToPixels(0f, 0f)
            : new Rem(3f).ToPixels();
        node.PreferredHeight = headerH;

        node.Paint = (rect, paintChildren) => {
            Style s = node.GetResolvedStyle();
            LightweaveWindowContext.PublishHeader(rect, draggable, ownsClose: showClose);

            BackgroundSpec bg = s.Background ?? BackgroundSpec.Of(ThemeSlot.SurfaceRaised);
            RadiusSpec? effectiveRadius = s.Radius ?? Runtime.LightweaveWindowContext.RequestedHeaderRadius;
            PaintBox.Draw(rect, bg, null, effectiveRadius);

            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;
            float pad = SpacingScale.Md.ToPixels();

            Rect closeRect = default;
            if (showClose) {
                closeRect = ComputeCloseRect(rect, rtl);
                LightweaveHitTracker.Track(closeRect);
            }

            if (!string.IsNullOrEmpty(title)) {
                Color textColor = s.TextColor switch {
                    ColorRef.Literal lit => lit.Value,
                    ColorRef.Token tok => theme.GetColor(tok.Slot),
                    _ => theme.GetColor(ThemeSlot.TextPrimary),
                };
                Color prev = GUI.color;
                GUI.color = textColor;
                Font font = theme.GetFont(FontRole.Heading);
                int pixelSize = Mathf.RoundToInt(new Rem(1.125f).ToFontPx());
                GUIStyle gstyle = GuiStyleCache.GetOrCreate(font, pixelSize);
                gstyle.clipping = TextClipping.Clip;
                float closeReserve = showClose ? new Rem(2.5f).ToPixels() : 0f;
                Rect titleRect;
                if (rtl) {
                    gstyle.alignment = TextAnchor.MiddleRight;
                    titleRect = new Rect(rect.x + closeReserve, rect.y, rect.width - pad - closeReserve, rect.height);
                }
                else {
                    gstyle.alignment = TextAnchor.MiddleLeft;
                    titleRect = new Rect(rect.x + pad, rect.y, rect.width - pad - closeReserve, rect.height);
                }

                GUI.Label(RectSnap.Snap(titleRect), title!, gstyle);
                GUI.color = prev;
            }

            if (showClose) {
                Color accent = theme.GetColor(ThemeSlot.SurfaceAccent);
                accent.a = 1f;
                Color baseColor;
                Color hoverColor;
                switch (closeStyle) {
                    case CloseButtonVariant.Primary:
                        baseColor = accent;
                        hoverColor = Color.Lerp(accent, Color.white, 0.2f);
                        break;
                    case CloseButtonVariant.Black:
                        baseColor = Color.black;
                        hoverColor = accent;
                        break;
                    default:
                        baseColor = theme.GetColor(ThemeSlot.TextPrimary);
                        hoverColor = accent;
                        break;
                }

                if (Widgets.ButtonImage(closeRect, TexButton.CloseXSmall, baseColor, hoverColor, true, null)) {
                    onClose?.Invoke();
                }

                MouseoverSounds.DoRegion(closeRect);
            }

            if (drawDivider) {
                float t = 1f;
                Rect lineRect = new Rect(rect.x, rect.yMax - t, rect.width, t);
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
                Color savedDiv = GUI.color;
                GUI.color = borderColor;
                GUI.DrawTexture(RectSnap.Snap(lineRect), BaseContent.WhiteTex);
                GUI.color = savedDiv;
            }
        };
        return node;
    }

    private static Rect ComputeCloseRect(Rect headerRect, bool rtl) {
        const float padding = 12f;
        const float size = 18f;
        float x = rtl ? headerRect.x + padding : headerRect.xMax - size - padding;
        return new Rect(x, headerRect.y + padding, size, size);
    }

}
