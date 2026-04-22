using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Input;

public static class Checkbox
{
    public static LightweaveNode Create(
        string label,
        bool value,
        Action<bool> onChange,
        bool disabled = false,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New($"Checkbox:{label}", line, file);

        node.Paint = (rect, paintChildren) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            float boxSize = new Rem(1.25f).ToPixels();
            float rowHeight = new Rem(1.75f).ToPixels();
            float gapPx = new Rem(0.5f).ToPixels();
            float rowY = rect.y + Mathf.Max(0f, (Mathf.Min(rect.height, rowHeight) - rowHeight) / 2f);
            float boxY = rowY + (rowHeight - boxSize) / 2f;

            float boxX = rtl ? rect.xMax - boxSize : rect.x;
            Rect boxRect = new Rect(boxX, boxY, boxSize, boxSize);

            float labelX = rtl ? rect.x : boxX + boxSize + gapPx;
            float labelWidth = rtl
                ? boxX - gapPx - rect.x
                : rect.xMax - labelX;
            Rect labelRect = new Rect(labelX, rowY, Mathf.Max(0f, labelWidth), rowHeight);

            Rect hitRect = new Rect(rect.x, rowY, rect.width, rowHeight);

            BackgroundSpec boxBg = value
                ? new BackgroundSpec.Solid(disabled ? ThemeSlot.SurfaceDisabled : ThemeSlot.SurfaceAccent)
                : new BackgroundSpec.Solid(disabled ? ThemeSlot.SurfaceDisabled : ThemeSlot.SurfaceInput);
            ThemeSlot borderSlot = disabled ? ThemeSlot.BorderSubtle : ThemeSlot.BorderDefault;
            BorderSpec boxBorder = BorderSpec.All(new Rem(1f / 16f), borderSlot);
            RadiusSpec boxRadius = RadiusSpec.All(new Rem(0.125f));

            PaintBox.Draw(boxRect, boxBg, boxBorder, boxRadius);

            if (value)
            {
                Font checkFont = theme.GetFont(FontRole.BodyBold);
                int checkSize = Mathf.RoundToInt(new Rem(1f).ToPixels());
                GUIStyle checkStyle = GuiStyleCache.Get(checkFont, checkSize, FontStyle.Bold);
                checkStyle.alignment = TextAnchor.MiddleCenter;
                Color savedCheck = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextOnAccent);
                GUI.Label(RectSnap.Snap(boxRect), "✓", checkStyle);
                GUI.color = savedCheck;
            }

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
