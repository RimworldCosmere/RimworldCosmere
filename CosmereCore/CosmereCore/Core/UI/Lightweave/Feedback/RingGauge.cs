using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Feedback;

public static class RingGauge
{
    public static LightweaveNode Create(
        float value,
        string? centerLabel = null,
        Rem thickness = default,
        ThemeSlot? fillColor = null,
        ThemeSlot? trackColor = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        Rem resolvedThickness = thickness.Equals(default(Rem)) ? new Rem(0.25f) : thickness;
        ThemeSlot resolvedFill = fillColor ?? ThemeSlot.SurfaceAccent;
        ThemeSlot resolvedTrack = trackColor ?? ThemeSlot.BorderDefault;

        LightweaveNode node = NodeBuilder.New("RingGauge", line, file);

        node.Paint = (rect, paintChildren) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;

            float clamped = Mathf.Clamp01(value);
            float cx = rect.x + rect.width * 0.5f;
            float cy = rect.y + rect.height * 0.5f;
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f - resolvedThickness.ToPixels() * 0.5f;
            float lineWidth = resolvedThickness.ToPixels();

            Color trackCol = theme.GetColor(resolvedTrack);
            Color fillCol = theme.GetColor(resolvedFill);

            Color saved = GUI.color;

            // Draw track (full circle, 60 segments)
            int totalSegments = 60;
            float segStep = 360f / totalSegments;

            GUI.color = trackCol;
            for (int i = 0; i < totalSegments; i++)
            {
                float a0 = i * segStep;
                float a1 = (i + 1) * segStep;
                float rad0 = a0 * Mathf.Deg2Rad;
                float rad1 = a1 * Mathf.Deg2Rad;
                Vector2 p0 = new Vector2(cx + Mathf.Sin(rad0) * radius, cy - Mathf.Cos(rad0) * radius);
                Vector2 p1 = new Vector2(cx + Mathf.Sin(rad1) * radius, cy - Mathf.Cos(rad1) * radius);
                Verse.Widgets.DrawLine(p0, p1, trackCol, lineWidth);
            }

            // Draw fill arc (clockwise from 12 o'clock)
            if (clamped > 0f)
            {
                int fillSegments = Mathf.Max(1, Mathf.RoundToInt(clamped * totalSegments));
                GUI.color = fillCol;
                for (int i = 0; i < fillSegments; i++)
                {
                    float a0 = i * segStep;
                    float a1 = Mathf.Min((i + 1) * segStep, clamped * 360f);
                    float rad0 = a0 * Mathf.Deg2Rad;
                    float rad1 = a1 * Mathf.Deg2Rad;
                    Vector2 p0 = new Vector2(cx + Mathf.Sin(rad0) * radius, cy - Mathf.Cos(rad0) * radius);
                    Vector2 p1 = new Vector2(cx + Mathf.Sin(rad1) * radius, cy - Mathf.Cos(rad1) * radius);
                    Verse.Widgets.DrawLine(p0, p1, fillCol, lineWidth);
                }
            }

            GUI.color = saved;

            // Draw centered label
            if (!string.IsNullOrEmpty(centerLabel))
            {
                Font font = theme.GetFont(FontRole.Body);
                int pixelSize = Mathf.RoundToInt(new Rem(0.75f).ToPixels());
                GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Normal);
                style.alignment = TextAnchor.MiddleCenter;

                Color labelColor = theme.GetColor(ThemeSlot.TextPrimary);
                GUI.color = labelColor;
                GUI.Label(RectSnap.Snap(rect), centerLabel, style);
                GUI.color = saved;
            }

            paintChildren();
        };

        return node;
    }
}
