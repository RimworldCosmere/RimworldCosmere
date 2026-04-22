using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Input;

public static class Switch
{
    public static LightweaveNode Create(
        string label,
        bool value,
        Action<bool> onChange,
        bool disabled = false,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New($"Switch:{label}", line, file);

        node.Paint = (rect, paintChildren) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            float trackWidth = new Rem(2.5f).ToPixels();
            float trackHeight = new Rem(1.25f).ToPixels();
            float thumbSize = new Rem(1f).ToPixels();
            float thumbInset = new Rem(0.125f).ToPixels();
            float rowHeight = new Rem(1.75f).ToPixels();
            float gapPx = new Rem(0.5f).ToPixels();

            float rowY = rect.y;
            float trackY = rowY + (rowHeight - trackHeight) / 2f;

            float trackX = rtl ? rect.xMax - trackWidth : rect.x;
            Rect trackRect = new Rect(trackX, trackY, trackWidth, trackHeight);

            float labelX = rtl ? rect.x : trackX + trackWidth + gapPx;
            float labelWidth = rtl
                ? trackX - gapPx - rect.x
                : rect.xMax - labelX;
            Rect labelRect = new Rect(labelX, rowY, Mathf.Max(0f, labelWidth), rowHeight);

            Rect hitRect = new Rect(rect.x, rowY, rect.width, rowHeight);

            ThemeSlot trackSlot = disabled
                ? ThemeSlot.SurfaceDisabled
                : value
                    ? ThemeSlot.SurfaceAccent
                    : ThemeSlot.SurfaceRaised;
            BackgroundSpec trackBg = new BackgroundSpec.Solid(trackSlot);
            ThemeSlot trackBorderSlot = disabled ? ThemeSlot.BorderSubtle : ThemeSlot.BorderDefault;
            BorderSpec trackBorder = BorderSpec.All(new Rem(1f / 16f), trackBorderSlot);
            RadiusSpec trackRadius = RadiusSpec.All(new Rem(0.625f));

            PaintBox.Draw(trackRect, trackBg, trackBorder, trackRadius);

            // Logical start = off, logical end = on. In LTR, end is right; in RTL, end is left.
            bool thumbAtEnd = value;
            float thumbX;
            if (rtl)
            {
                thumbX = thumbAtEnd
                    ? trackRect.x + thumbInset
                    : trackRect.xMax - thumbInset - thumbSize;
            }
            else
            {
                thumbX = thumbAtEnd
                    ? trackRect.xMax - thumbInset - thumbSize
                    : trackRect.x + thumbInset;
            }
            float thumbY = trackRect.y + (trackHeight - thumbSize) / 2f;
            Rect thumbRect = new Rect(thumbX, thumbY, thumbSize, thumbSize);

            ThemeSlot thumbSlot = value
                ? ThemeSlot.TextOnAccent
                : ThemeSlot.SurfacePrimary;
            BackgroundSpec thumbBg = new BackgroundSpec.Solid(thumbSlot);
            RadiusSpec thumbRadius = RadiusSpec.All(new Rem(0.5f));
            PaintBox.Draw(thumbRect, thumbBg, null, thumbRadius);

            Font labelFont = theme.GetFont(FontRole.Body);
            int labelPixelSize = Mathf.RoundToInt(new Rem(1f).ToPixels());
            GUIStyle labelStyle = GuiStyleCache.Get(labelFont, labelPixelSize, FontStyle.Normal);
            labelStyle.alignment = rtl ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            Color labelColor = disabled
                ? theme.GetColor(ThemeSlot.TextMuted)
                : theme.GetColor(ThemeSlot.TextPrimary);
            Color savedLabel = GUI.color;
            GUI.color = labelColor;
            GUI.Label(RectSnap.Snap(labelRect), label, labelStyle);
            GUI.color = savedLabel;

            paintChildren();

            Event e = Event.current;
            if (!disabled && e.type == EventType.MouseUp && e.button == 0 && hitRect.Contains(e.mousePosition))
            {
                onChange?.Invoke(!value);
                e.Use();
            }
        };

        return node;
    }
}
