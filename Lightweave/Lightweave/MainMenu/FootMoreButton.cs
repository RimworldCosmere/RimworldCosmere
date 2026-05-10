using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Hooks;
using static Cosmere.Lightweave.Hooks.Hooks;
using Cosmere.Lightweave.Navigation;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.MainMenu;

public static class FootMoreButton {
    private static readonly Rem MenuWidth = new Rem(18f);

    public static LightweaveNode Create(
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        StateHandle<bool> open = UseState(false, line, file);
        StateHandle<Rect> anchor = UseState(Rect.zero, line + 1, file);

        LightweaveNode node = NodeBuilder.New("FootMoreButton", line, file);
        node.PreferredHeight = new Rem(2f).ToPixels();

        LightweaveNode trigger = FootLink.Create(
            label: "CL_MainMenu_More".Translate(),
            onClick: () => open.Set(!open.Value),
            indicateMenu: true
        );

        LightweaveNode menu = Menu.Create(
            isOpen: open.Value,
            anchorRect: anchor.Value,
            items: MoreButton.BuildItems(() => open.Set(false)),
            onDismiss: () => open.Set(false),
            anchor: MenuAnchor.Left,
            direction: MenuDirection.Up,
            instanceKey: "foot-more-menu",
            header: (string)"CL_MainMenu_More_Header".Translate(),
            headerMeta: null,
            searchPlaceholder: null,
            size: new Vector2(MenuWidth.ToPixels(), -1f)
        );

        node.MeasureWidth = () => trigger.MeasureWidth?.Invoke() ?? new Rem(5f).ToPixels();
        node.Children.Add(trigger);
        node.Children.Add(menu);

        node.Paint = (rect, _) => {
            anchor.Set(rect);
            trigger.MeasuredRect = rect;
            menu.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(trigger, rect);
            LightweaveRoot.PaintSubtree(menu, rect);
        };

        return node;
    }
}
