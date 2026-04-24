using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Data;

public static class List {
    public static LightweaveNode Create<T>(
        IReadOnlyList<T> items,
        Func<T, int, LightweaveNode> rowBuilder,
        float? rowHeight = null,
        Func<T, object>? keyFn = null,
        bool virtualize = true,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Hooks.Hooks.RefHandle<LightweaveScrollStatus> statusRef =
            Hooks.Hooks.UseRef(new LightweaveScrollStatus(), line, file);

        LightweaveNode node = NodeBuilder.New($"List<{typeof(T).Name}>", line, file);

        if (rowHeight.HasValue && items != null) {
            node.PreferredHeight = items.Count * rowHeight.Value;
        }

        node.Paint = (rect, paintChildren) => {
            if (items == null) {
                return;
            }

            bool doVirtualize = virtualize && rowHeight.HasValue;
            float totalHeight = rowHeight.HasValue
                ? items.Count * rowHeight.Value
                : items.Count * 36f;

            statusRef.Current.Height = totalHeight;
            using (new LightweaveScrollView(rect, statusRef.Current)) {
                float scrollbarGutter = LightweaveScrollView.GutterPixels(statusRef.Current.VerticalVisible);
                float innerWidth = rect.width - scrollbarGutter;

                node.Children.Clear();

                if (doVirtualize) {
                    float rh = Mathf.Max(1f, rowHeight!.Value);
                    float scrollY = statusRef.Current.Position.y;

                    int startIdx = Math.Max(0, (int)Math.Floor(scrollY / rh) - 2);
                    int endIdx = Math.Min(items.Count, (int)Math.Ceiling((scrollY + rect.height) / rh) + 2);

                    for (int i = startIdx; i < endIdx; i++) {
                        LightweaveNode row = rowBuilder(items[i], i);
                        row.ExplicitKey = keyFn?.Invoke(items[i]) ?? i;
                        row.MeasuredRect = new Rect(0f, i * rh, innerWidth, rh);
                        node.Children.Add(row);
                    }
                } else {
                    float rh = rowHeight ?? 36f;
                    for (int i = 0; i < items.Count; i++) {
                        LightweaveNode row = rowBuilder(items[i], i);
                        row.ExplicitKey = keyFn?.Invoke(items[i]) ?? i;
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