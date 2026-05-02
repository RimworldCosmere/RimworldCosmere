using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Hooks.Hooks;
using static Cosmere.Lightweave.Typography.Typography;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Overlay;

[Doc(
    Id = "dialog",
    Summary = "Modal overlay window for confirmations, forms, or focused tasks.",
    WhenToUse = "Block the rest of the UI while the player resolves a single decision.",
    SourcePath = "Lightweave/Lightweave/Overlay/Dialog.cs"
)]
public static class Dialog {
    public static LightweaveNode Create(
        [DocParam("Whether the dialog is currently visible.")]
        bool isOpen,
        [DocParam("Invoked when the dialog requests dismissal.")]
        Action onClose,
        [DocParam("Builds the dialog content node.")]
        Func<LightweaveNode> content,
        [DocParam("Initial window size in pixels. Height of -1 auto-sizes to content.")]
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
        return Layout.Layout.Card.Create(children);
    }

    public static LightweaveNode Header(params LightweaveNode[] children) {
        return Layout.Layout.Card.Header(children);
    }

    public static LightweaveNode Title(
        string text,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        return Layout.Layout.Card.Title(text, line, file);
    }

    public static LightweaveNode Description(
        string text,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        return Layout.Layout.Card.Description(text, line, file);
    }

    public static LightweaveNode Content(params LightweaveNode[] children) {
        return Layout.Layout.Card.Content(children);
    }

    public static LightweaveNode Footer(params LightweaveNode[] children) {
        return Layout.Layout.Card.Footer(children);
    }

    private static LightweaveNode BuildHostDemo() {
        StateHandle<bool> open = UseState(false);

        LightweaveNode trigger = Button.Create(
            (string)"CC_Playground_Dialog_TriggerOpen".Translate(),
            () => open.Set(true)
        );

        LightweaveNode dialog = Create(
            open.Value,
            () => open.Set(false),
            () => Root(
                Header(
                    Title((string)"CC_Playground_Overlay_Dialog_Header".Translate()),
                    Description((string)"CC_Playground_Overlay_Dialog_Body".Translate())
                ),
                Content(
                    Text.Create(
                        (string)"CC_Playground_Overlay_Dialog_Body".Translate(),
                        FontRole.Body,
                        new Rem(0.9375f),
                        ThemeSlot.TextPrimary
                    )
                ),
                Footer(
                    Button.Create(
                        (string)"CC_Playground_Label_Confirm".Translate(),
                        () => open.Set(false)
                    )
                )
            )
        );

        LightweaveNode composed = NodeBuilder.New("DialogHost", 0, nameof(Dialog));
        composed.Children.Add(trigger);
        composed.Children.Add(dialog);
        composed.Measure = w => trigger.Measure?.Invoke(w) ?? trigger.PreferredHeight ?? 32f;
        composed.Paint = (rect, _) => {
            trigger.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(trigger, rect);
            dialog.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(dialog, rect);
        };
        return composed;
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(BuildHostDemo(), useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(BuildHostDemo(), useFullSource: true);
    }
}