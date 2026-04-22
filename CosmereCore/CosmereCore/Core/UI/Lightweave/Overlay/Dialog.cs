using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Overlay;

public static class Dialog
{
    public static LightweaveNode Create(
        bool isOpen,
        Action onClose,
        Func<LightweaveNode> title,
        Func<LightweaveNode> body,
        Func<LightweaveNode>? footer = null,
        Vector2? size = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        Vector2 resolvedSize = size ?? new Vector2(new Rem(28f).ToPixels(), new Rem(18f).ToPixels());
        Hooks.Hooks.RefHandle<DialogWindow?> windowRef = Hooks.Hooks.UseRef<DialogWindow?>(null, line, file);

        LightweaveNode node = NodeBuilder.New("Dialog", line, file);
        node.Paint = (_, _) =>
        {
            DialogWindow? current = windowRef.Current;
            bool inStack = current != null && Find.WindowStack.IsOpen(current);

            if (isOpen)
            {
                if (current == null || !inStack)
                {
                    DialogWindow fresh = new DialogWindow(title, body, footer, onClose, resolvedSize);
                    Find.WindowStack.Add(fresh);
                    windowRef.Current = fresh;
                }
                return;
            }

            if (current != null)
            {
                if (inStack)
                {
                    current.Close();
                }
                windowRef.Current = null;
            }
        };
        return node;
    }
}
