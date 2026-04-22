using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Navigation;

public static class Tabs
{
    public static LightweaveNode Create<T>(
        T value,
        IReadOnlyList<T> items,
        Func<T, string> labelFn,
        Action<T> onChange,
        Func<T, LightweaveNode> bodyFn,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New($"Tabs<{typeof(T).Name}>", line, file);
        LightweaveNode bodyNode = bodyFn(value);
        node.Children.Add(bodyNode);

        node.Paint = (rect, _) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            float barHeight = new Rem(2.5f).ToPixels();
            float dividerThickness = new Rem(1f / 16f).ToPixels();
            float underlineThickness = new Rem(2f / 16f).ToPixels();
            float tabGap = new Rem(0.25f).ToPixels();
            float tabPadding = SpacingScale.Md.ToPixels();

            Rect barRect = new Rect(rect.x, rect.y, rect.width, barHeight);
            Rect dividerRect = new Rect(rect.x, barRect.yMax, rect.width, dividerThickness);
            Rect bodyRect = new Rect(
                rect.x,
                dividerRect.yMax,
                rect.width,
                Mathf.Max(0f, rect.yMax - dividerRect.yMax));

            Font font = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToPixels());
            GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleCenter;

            int count = items.Count;
            float[] widths = new float[count];
            for (int i = 0; i < count; i++)
            {
                string label = labelFn(items[i]);
                Vector2 labelSize = style.CalcSize(new GUIContent(label));
                widths[i] = labelSize.x + tabPadding * 2f;
            }

            Event e = Event.current;
            Color savedColor = GUI.color;

            float cursor = rtl ? barRect.xMax : barRect.x;
            for (int i = 0; i < count; i++)
            {
                T item = items[i];
                bool active = EqualityComparer<T>.Default.Equals(item, value);
                float tabWidth = widths[i];

                Rect tabRect;
                if (rtl)
                {
                    tabRect = new Rect(cursor - tabWidth, barRect.y, tabWidth, barRect.height);
                    cursor -= tabWidth + tabGap;
                }
                else
                {
                    tabRect = new Rect(cursor, barRect.y, tabWidth, barRect.height);
                    cursor += tabWidth + tabGap;
                }

                bool hovering = tabRect.Contains(e.mousePosition);
                ThemeSlot textSlot = active || hovering ? ThemeSlot.TextPrimary : ThemeSlot.TextSecondary;

                GUI.color = theme.GetColor(textSlot);
                GUI.Label(RectSnap.Snap(tabRect), labelFn(item), style);
                GUI.color = savedColor;

                if (active)
                {
                    Rect underlineRect = new Rect(
                        tabRect.x,
                        tabRect.yMax - underlineThickness,
                        tabRect.width,
                        underlineThickness);
                    PaintBox.Draw(underlineRect, new BackgroundSpec.Solid(ThemeSlot.SurfaceAccent), null, null);
                }

                if (e.type == EventType.MouseUp && e.button == 0 && tabRect.Contains(e.mousePosition))
                {
                    onChange?.Invoke(item);
                    e.Use();
                }
            }

            PaintBox.Draw(dividerRect, new BackgroundSpec.Solid(ThemeSlot.BorderSubtle), null, null);

            bodyNode.MeasuredRect = bodyRect;
            LightweaveRoot.PaintSubtree(bodyNode, bodyRect);
        };

        return node;
    }
}
