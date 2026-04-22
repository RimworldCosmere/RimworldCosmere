using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Runtime;

namespace Cosmere.Core.UI.Lightweave.Data;

public static class List
{
    public static LightweaveNode Create<T>(
        IReadOnlyList<T> items,
        Func<T, int, LightweaveNode> rowBuilder,
        float? rowHeight = null,
        Func<T, object>? keyFn = null,
        bool virtualize = true,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        Hooks.Hooks.RefHandle<ScrollViewStatus> statusRef =
            Hooks.Hooks.UseRef(new ScrollViewStatus(), line, file);

        LightweaveNode node = NodeBuilder.New($"List<{typeof(T).Name}>", line, file);

        node.Paint = (rect, paintChildren) =>
        {
            bool doVirtualize = virtualize && rowHeight.HasValue;
            float totalHeight = rowHeight.HasValue
                ? items.Count * rowHeight.Value
                : items.Count * 32f;

            statusRef.Current.height = totalHeight;
            using (new ScrollView(rect, statusRef.Current))
            {
                float scrollbarGutter = statusRef.Current.scrollVisibile ? 20f : 0f;
                float innerWidth = rect.width - scrollbarGutter;

                node.Children.Clear();

                if (doVirtualize)
                {
                    float rh = Mathf.Max(1f, rowHeight!.Value);
                    float scrollY = statusRef.Current.position.y;

                    int startIdx = Math.Max(0, (int)Math.Floor(scrollY / rh) - 2);
                    int endIdx = Math.Min(items.Count, (int)Math.Ceiling((scrollY + rect.height) / rh) + 2);

                    for (int i = startIdx; i < endIdx; i++)
                    {
                        LightweaveNode row = rowBuilder(items[i], i);
                        row.ExplicitKey = keyFn?.Invoke(items[i]) ?? (object)i;
                        row.MeasuredRect = new Rect(0f, i * rh, innerWidth, rh);
                        node.Children.Add(row);
                    }
                }
                else
                {
                    float rh = rowHeight ?? 32f;
                    for (int i = 0; i < items.Count; i++)
                    {
                        LightweaveNode row = rowBuilder(items[i], i);
                        row.ExplicitKey = keyFn?.Invoke(items[i]) ?? (object)i;
                        row.MeasuredRect = new Rect(0f, i * rh, innerWidth, rh);
                        node.Children.Add(row);
                    }
                }

                paintChildren();
            }
        };

        return node;
    }
}
