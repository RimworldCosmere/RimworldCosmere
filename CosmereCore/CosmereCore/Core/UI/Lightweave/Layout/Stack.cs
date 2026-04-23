using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Layout;

public sealed class StackBuilder
{
    internal readonly List<(LightweaveNode node, float height)> Items = new List<(LightweaveNode, float)>();

    public void Add(LightweaveNode node, float height)
    {
        Items.Add((node, height));
    }
}

public static partial class Layout
{
    public static LightweaveNode Stack(
        Rem gap = default,
        Action<StackBuilder>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        StackBuilder builder = new StackBuilder();
        children?.Invoke(builder);

        LightweaveNode node = NodeBuilder.New("Stack", line, file);
        for (int i = 0; i < builder.Items.Count; i++)
        {
            node.Children.Add(builder.Items[i].node);
        }

        float gapPx = gap.ToPixels();
        float total = 0f;
        for (int i = 0; i < builder.Items.Count; i++)
        {
            total += builder.Items[i].height;
        }
        if (builder.Items.Count > 1)
        {
            total += gapPx * (builder.Items.Count - 1);
        }
        node.PreferredHeight = total;

        node.Paint = (rect, paintChildren) =>
        {
            float y = rect.y;
            for (int i = 0; i < builder.Items.Count; i++)
            {
                (LightweaveNode child, float h) = builder.Items[i];
                child.MeasuredRect = new Rect(rect.x, y, rect.width, h);
                y += h + gapPx;
            }
            paintChildren();
        };
        return node;
    }
}
