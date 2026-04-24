using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout {
    public static LightweaveNode ScrollArea(
        LightweaveNode content,
        object? resetKey = null,
        bool showScrollbar = true,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Hooks.Hooks.RefHandle<LightweaveScrollStatus> statusRef =
            Hooks.Hooks.UseRef(new LightweaveScrollStatus(), line, file);
        Hooks.Hooks.RefHandle<object?> lastResetKey = Hooks.Hooks.UseRef<object?>(null, line, file + "#resetKey");

        if (!Equals(lastResetKey.Current, resetKey)) {
            statusRef.Current.Position = Vector2.zero;
            lastResetKey.Current = resetKey;
        }

        LightweaveNode node = NodeBuilder.New("ScrollArea", line, file);
        node.Children.Add(content);
        node.Paint = (rect, paintChildren) => {
            float scrollbarGutter = LightweaveScrollView.GutterPixels(statusRef.Current.VerticalVisible);
            float innerWidth = rect.width - scrollbarGutter;
            float contentHeight = content.Measure?.Invoke(innerWidth) ?? content.PreferredHeight ?? rect.height;
            statusRef.Current.Height = contentHeight;
            using (new LightweaveScrollView(rect, statusRef.Current, showScrollbar)) {
                content.MeasuredRect = new Rect(0f, 0f, innerWidth, contentHeight);
                paintChildren();
            }
        };
        return node;
    }
}