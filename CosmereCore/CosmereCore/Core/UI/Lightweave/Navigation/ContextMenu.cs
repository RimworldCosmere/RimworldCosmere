using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Navigation;

public static class ContextMenu {
    public static LightweaveNode Create(
        LightweaveNode child,
        IReadOnlyList<MenuItem> items,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Hooks.Hooks.StateHandle<bool> isOpen = Hooks.Hooks.UseState(false, line, file + "#open");
        Hooks.Hooks.StateHandle<Vector2> anchorPos = Hooks.Hooks.UseState(Vector2.zero, line, file + "#pos");

        LightweaveNode node = NodeBuilder.New("ContextMenu", line, file);
        node.Children.Add(child);
        if (child.Measure != null) {
            node.Measure = child.Measure;
        } else if (child.PreferredHeight.HasValue) {
            node.PreferredHeight = child.PreferredHeight.Value;
        }

        node.Paint = (rect, _) => {
            child.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(child, rect);

            Event e = Event.current;
            if (e.type == EventType.MouseUp && e.button == 1 && rect.Contains(e.mousePosition)) {
                anchorPos.Set(e.mousePosition);
                isOpen.Set(true);
                e.Use();
            }

            if (isOpen.Value) {
                Vector2 pos = anchorPos.Value;
                Rect anchorRect = new Rect(pos.x, pos.y, 0f, 0f);
                LightweaveNode menu = Menu.Create(
                    true,
                    anchorRect,
                    items,
                    () => isOpen.Set(false),
                    MenuAnchor.Left,
                    MenuDirection.Down,
                    "context"
                );
                menu.MeasuredRect = rect;
                LightweaveRoot.PaintSubtree(menu, rect);
            }
        };
        return node;
    }
}