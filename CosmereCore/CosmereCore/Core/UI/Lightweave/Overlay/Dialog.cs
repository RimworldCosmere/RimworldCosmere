using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Surface;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Overlay;

public static class Dialog {
    public static LightweaveNode Create(
        bool isOpen,
        Action onClose,
        Func<LightweaveNode> content,
        Vector2? size = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Vector2 resolvedSize = size ?? new Vector2(new Rem(32f).ToPixels(), -1f);
        Hooks.Hooks.RefHandle<DialogWindow?> windowRef = Hooks.Hooks.UseRef<DialogWindow?>(null, line, file);

        LightweaveNode node = NodeBuilder.New("Dialog", line, file);
        node.Paint = (_, _) => {
            DialogWindow? current = windowRef.Current;
            bool inStack = current != null && Find.WindowStack.IsOpen(current);

            if (isOpen) {
                if (current == null || !inStack) {
                    DialogWindow fresh = new DialogWindow(content, onClose, resolvedSize);
                    Find.WindowStack.Add(fresh);
                    windowRef.Current = fresh;
                }

                return;
            }

            if (current != null) {
                if (inStack) {
                    current.Close();
                }

                windowRef.Current = null;
            }
        };
        return node;
    }

    public static LightweaveNode Root(params LightweaveNode[] children) {
        return Surface.Surface.Card.Create(children);
    }

    public static LightweaveNode Header(params LightweaveNode[] children) {
        return Surface.Surface.Card.Header(children);
    }

    public static LightweaveNode Title(
        string text,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        return Surface.Surface.Card.Title(text, line, file);
    }

    public static LightweaveNode Description(
        string text,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        return Surface.Surface.Card.Description(text, line, file);
    }

    public static LightweaveNode Content(params LightweaveNode[] children) {
        return Surface.Surface.Card.Content(children);
    }

    public static LightweaveNode Footer(params LightweaveNode[] children) {
        return Surface.Surface.Card.Footer(children);
    }
}