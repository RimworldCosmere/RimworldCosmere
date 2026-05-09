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
        [DocParam("Title text shown left-aligned (right-aligned in RTL).")]
        string? title = null,
        [DocParam("Render an X close button on the trailing edge.")]
        bool showClose = true,
        [DocParam("Action invoked when the close button is clicked.")]
        Action? onClose = null,
        [DocParam("Mark the header rect as the host window's drag region.")]
        bool draggable = true,
        [DocParam("Header band height.", TypeOverride = "Rem", DefaultOverride = "3rem")]
        Rem? height = null,
        [DocParam("Theme slot used for the header background fill.")]
        ThemeSlot backgroundSlot = ThemeSlot.SurfaceRaised,
        [DocParam("Theme slot for the title text color.")]
        ThemeSlot textSlot = ThemeSlot.TextPrimary,
        [DocParam("Draw a 1px divider beneath the header band.")]
        bool drawDivider = true,
        [DocParam("Theme slot for the divider color.")]
        ThemeSlot dividerSlot = ThemeSlot.BorderDefault,
        [DocParam("Close button color treatment.")]
        CloseButtonVariant closeStyle = CloseButtonVariant.Default,
        [DocParam("Override corner radius for the header background. When null, falls back to the host window's requested top-rounding (so headers tuck under bordered windows automatically).")]
        RadiusSpec? cornerRadius = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Rem h = height ?? new Rem(3f);
        float headerH = h.ToPixels();

        LightweaveNode node = NodeBuilder.New("WindowHeader", line, file);
        node.PreferredHeight = headerH;
        node.Paint = (rect, paintChildren) => {
            LightweaveWindowContext.PublishHeader(rect, draggable, ownsClose: showClose);

            RadiusSpec? effectiveRadius = cornerRadius ?? Runtime.LightweaveWindowContext.RequestedHeaderRadius;
            PaintBox.Draw(rect, BackgroundSpec.Of(backgroundSlot), null, effectiveRadius);

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
                Color textColor = theme.GetColor(textSlot);
                Color prev = GUI.color;
                GUI.color = textColor;
                Font font = theme.GetFont(FontRole.Heading);
                int pixelSize = Mathf.RoundToInt(new Rem(1.125f).ToFontPx());
                GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Bold);
                style.clipping = TextClipping.Clip;
                float closeReserve = showClose ? new Rem(2.5f).ToPixels() : 0f;
                Rect titleRect;
                if (rtl) {
                    style.alignment = TextAnchor.MiddleRight;
                    titleRect = new Rect(rect.x + closeReserve, rect.y, rect.width - pad - closeReserve, rect.height);
                }
                else {
                    style.alignment = TextAnchor.MiddleLeft;
                    titleRect = new Rect(rect.x + pad, rect.y, rect.width - pad - closeReserve, rect.height);
                }

                GUI.Label(RectSnap.Snap(titleRect), title!, style);
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
                Rect line = new Rect(rect.x, rect.yMax - t, rect.width, t);
                Color borderColor = theme.GetColor(dividerSlot);
                Color savedDiv = GUI.color;
                GUI.color = borderColor;
                GUI.DrawTexture(RectSnap.Snap(line), BaseContent.WhiteTex);
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
