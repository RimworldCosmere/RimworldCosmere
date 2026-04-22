using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Feedback;

public static class ProgressBar
{
    public static LightweaveNode Create(
        float value,
        float min = 0f,
        float max = 1f,
        string? label = null,
        BadgeVariant variant = BadgeVariant.Accent,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New($"ProgressBar:{variant}", line, file);

        node.Paint = (rect, paintChildren) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            RadiusSpec radius = RadiusSpec.All(new Rem(0.5f));

            BackgroundSpec trackBg = new BackgroundSpec.Solid(ThemeSlot.SurfaceInput);
            BorderSpec trackBorder = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
            PaintBox.Draw(rect, trackBg, trackBorder, radius);

            float range = max - min;
            float fraction = range > 0f ? Mathf.Clamp01((value - min) / range) : 0f;
            float fillWidth = rect.width * fraction;

            Rect fillRect = default(Rect);
            bool hasFill = fillWidth > 0f;
            if (hasFill)
            {
                float fillX = rtl ? rect.xMax - fillWidth : rect.x;
                fillRect = new Rect(fillX, rect.y, fillWidth, rect.height);
                BackgroundSpec fillBg = new BackgroundSpec.Solid(BadgeVariants.Background(variant));
                PaintBox.Draw(fillRect, fillBg, null, radius);
            }

            if (!string.IsNullOrEmpty(label))
            {
                Font font = theme.GetFont(FontRole.BodyBold);
                int pixelSize = Mathf.RoundToInt(new Rem(0.75f).ToPixels());
                GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Bold);
                style.alignment = TextAnchor.MiddleCenter;

                float midX = rect.x + rect.width / 2f;
                bool midOverFill = hasFill && midX >= fillRect.x && midX <= fillRect.xMax;
                ThemeSlot labelSlot = midOverFill
                    ? BadgeVariants.Foreground(variant)
                    : ThemeSlot.TextPrimary;

                Color savedColor = GUI.color;
                GUI.color = theme.GetColor(labelSlot);
                GUI.Label(RectSnap.Snap(rect), label, style);
                GUI.color = savedColor;
            }

            paintChildren();
        };

        return node;
    }
}
