using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Layout;

public sealed class HStackBuilder
{
    internal readonly List<(LightweaveNode node, float width, bool flex)> Items = new List<(LightweaveNode, float, bool)>();

    public void Add(LightweaveNode node, float width)
    {
        Items.Add((node, width, false));
    }

    public void AddFlex(LightweaveNode node)
    {
        Items.Add((node, 0f, true));
    }
}

public static partial class Layout
{
    public static LightweaveNode HStack(
        Rem gap = default,
        Action<HStackBuilder>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        HStackBuilder builder = new HStackBuilder();
        children?.Invoke(builder);

        LightweaveNode node = NodeBuilder.New("HStack", line, file);
        int count = builder.Items.Count;
        for (int i = 0; i < count; i++)
        {
            node.Children.Add(builder.Items[i].node);
        }

        float gapPx = gap.ToPixels();

        float? maxChildHeight = null;
        for (int i = 0; i < count; i++)
        {
            float? ph = builder.Items[i].node.PreferredHeight;
            if (ph.HasValue)
            {
                if (!maxChildHeight.HasValue || ph.Value > maxChildHeight.Value)
                {
                    maxChildHeight = ph.Value;
                }
            }
        }
        node.PreferredHeight = maxChildHeight;

        node.Paint = (rect, paintChildren) =>
        {
            if (count == 0)
            {
                return;
            }

            Direction dir = RenderContext.Current.Direction;
            bool reverse = dir == Direction.Rtl;

            float fixedTotal = 0f;
            int flexCount = 0;
            for (int i = 0; i < count; i++)
            {
                if (builder.Items[i].flex)
                {
                    flexCount++;
                }
                else
                {
                    fixedTotal += builder.Items[i].width;
                }
            }

            float totalGap = gapPx * Math.Max(0, count - 1);
            float remainingForFlex = Mathf.Max(0f, rect.width - fixedTotal - totalGap);
            float flexEach = flexCount > 0 ? remainingForFlex / flexCount : 0f;

            float x = rect.x;
            for (int i = 0; i < count; i++)
            {
                int idx = reverse ? (count - 1 - i) : i;
                (LightweaveNode child, float width, bool flex) = builder.Items[idx];
                float w = flex ? flexEach : width;
                child.MeasuredRect = new Rect(x, rect.y, w, rect.height);
                x += w + gapPx;
            }

            paintChildren();
        };

        return node;
    }
}
