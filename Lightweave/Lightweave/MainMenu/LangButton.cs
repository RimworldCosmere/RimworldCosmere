using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Hooks;
using static Cosmere.Lightweave.Hooks.Hooks;
using Cosmere.Lightweave.Overlay;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.MainMenu;

public static class LangButton {
    public static LightweaveNode Create(
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        StateHandle<bool> open = UseState(false, line, file);
        StateHandle<Rect> anchor = UseState(Rect.zero, line + 1, file);
        StateHandle<string> query = UseState(string.Empty, line + 2, file);

        LoadedLanguage active = LanguageDatabase.activeLanguage;
        string label = active != null ? (active.FriendlyNameNative ?? active.folderName) : "English";

        LightweaveNode node = NodeBuilder.New("LangButton", line, file);
        node.ApplyStyling("lang-button", style, classes, id);
        node.PreferredHeight = new Rem(2f).ToPixels();

        LightweaveNode trigger = FootLink.Create(
            label: label,
            onClick: () => open.Set(!open.Value),
            indicateMenu: true,
            expanded: open.Value
        );

        LightweaveNode popover = Popover.Create(
            isOpen: open.Value,
            anchorRect: anchor.Value,
            placement: PopoverPlacement.Top,
            content: LangPopover.Create(query.Value, q => query.Set(q), () => open.Set(false)),
            onDismiss: () => {
                open.Set(false);
                query.Set(string.Empty);
            },
            preferredSize: new Vector2(new Rem(21f).ToPixels(), -1f)
        );

        node.MeasureWidth = () => trigger.MeasureWidth?.Invoke() ?? new Rem(8f).ToPixels();
        node.Children.Add(trigger);
        node.Children.Add(popover);

        node.Paint = (rect, _) => {
            anchor.Set(rect);
            trigger.MeasuredRect = rect;
            popover.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(trigger, rect);
            LightweaveRoot.PaintSubtree(popover, rect);
        };

        return node;
    }
}
