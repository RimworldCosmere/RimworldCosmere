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
    WhenToUse = "Block the rest of the UI while the player resolves a single decision. The dialog is the overlay-window shell only; compose its body with Card.* primitives.",
    SourcePath = "Lightweave/Lightweave/Overlay/Dialog.cs"
)]
public static class Dialog {
    public static LightweaveNode Create(
        [DocParam("Whether the dialog is currently visible.")]
        bool isOpen,
        [DocParam("Invoked when the dialog requests dismissal.")]
        Action onDismiss,
        [DocParam("Builds the dialog content node.")]
        Func<LightweaveNode> content,
        [DocParam("Initial window size in pixels. Height of -1 auto-sizes to content.")]
        Vector2? size = null,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Vector2 resolvedSize = size ?? new Vector2(new Rem(32f).ToPixels(), -1f);
        Hooks.Hooks.RefHandle<DialogWindow?> windowRef = Hooks.Hooks.UseRef<DialogWindow?>(null, line, file);

        LightweaveNode node = NodeBuilder.New("Dialog", line, file);
        node.ApplyStyling("dialog", style, classes, id);
        node.Paint = (_, _) => {
            DialogWindow? current = windowRef.Current;
            bool inStack = current != null && Find.WindowStack.IsOpen(current);

            if (isOpen) {
                if (current == null || !inStack) {
                    DialogWindow fresh = new DialogWindow(content, onDismiss, resolvedSize);
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

    private static LightweaveNode BuildHostDemo() {
        StateHandle<bool> open = UseState(false);

        LightweaveNode trigger = Button.Create(
            (string)"CL_Playground_Dialog_TriggerOpen".Translate(),
            () => open.Set(true)
        );

        LightweaveNode dialog = Create(
            open.Value,
            () => open.Set(false),
            () => Layout.Card.Create(
                c => {
                    c.Add(Layout.Card.Header(h => {
                        h.Add(Layout.Card.Title((string)"CL_Playground_Overlay_Dialog_Header".Translate()));
                        h.Add(Layout.Card.Description((string)"CL_Playground_Overlay_Dialog_Body".Translate()));
                    }));
                    c.Add(Layout.Card.Content(ct => {
                        ct.Add(Text.Create(
                            (string)"CL_Playground_Overlay_Dialog_Body".Translate(),
                            style: new Style { FontFamily = FontRole.Body, FontSize = new Rem(0.9375f), TextColor = ThemeSlot.TextPrimary }
                        ));
                    }));
                    c.Add(Layout.Card.Footer(f => {
                        f.Add(Button.Create(
                            (string)"CL_Playground_Label_Confirm".Translate(),
                            () => open.Set(false)
                        ));
                    }));
                }
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

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => BuildHostDemo(), useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => BuildHostDemo(), useFullSource: true);
    }
}