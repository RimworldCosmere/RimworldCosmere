using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Doc.DocChips;

namespace Cosmere.Lightweave.Layout;

public static partial class Layout {
    [Doc(
        Id = "scrollarea",
        Summary = "Scrollable viewport that clips overflow content.",
        WhenToUse = "Wrap content that may exceed the available height.",
        SourcePath = "Lightweave/Lightweave/Layout/ScrollArea.cs",
        PreferredVariantHeight = 160f
    )]
    public static class ScrollArea {
        public static LightweaveNode Create(
            [DocParam("Content to scroll within the viewport.")]
            LightweaveNode content,
            [DocParam("Optional reset key. When changed, scroll position resets.", TypeOverride = "object?", DefaultOverride = "null")]
            object? resetKey = null,
            [DocParam("Show the vertical scrollbar gutter.")]
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

            return BuildScrollArea(content, statusRef.Current, showScrollbar, line, file);
        }

        public static LightweaveNode External(
            LightweaveNode content,
            LightweaveScrollStatus status,
            bool showScrollbar = true,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            return BuildScrollArea(content, status, showScrollbar, line, file);
        }

        private static LightweaveNode BuildScrollArea(
            LightweaveNode content,
            LightweaveScrollStatus status,
            bool showScrollbar,
            int line,
            string file
        ) {
            LightweaveNode node = NodeBuilder.New("ScrollArea", line, file);
            node.Children.Add(content);
            node.Paint = (rect, paintChildren) => {
                float scrollbarGutter = LightweaveScrollView.GutterPixels(status.VerticalVisible);
                float innerWidth = rect.width - scrollbarGutter;
                float contentHeight = content.Measure?.Invoke(innerWidth) ?? content.PreferredHeight ?? rect.height;
                status.Height = contentHeight;
                using (new LightweaveScrollView(rect, status, showScrollbar)) {
                    content.MeasuredRect = new Rect(0f, 0f, innerWidth, contentHeight);
                    paintChildren();
                }
            };
            return node;
        }

        private static LightweaveNode DocsRows(int count) {
            return Stack.Create(
                SpacingScale.Xxs,
                s => {
                    for (int i = 0; i < count; i++) {
                        s.Add(SampleChip("row " + (i + 1)), new Rem(1.75f).ToPixels());
                    }
                }
            );
        }

        [DocVariant("CC_Playground_ScrollArea_WithBar")]
        public static DocSample DocsWithBar() {
            return new DocSample(
                ScrollArea.Create(DocsRows(20))
            );
        }

        [DocVariant("CC_Playground_ScrollArea_NoBar")]
        public static DocSample DocsNoBar() {
            return new DocSample(
                ScrollArea.Create(DocsRows(20), showScrollbar: false)
            );
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(
                ScrollArea.Create(DocsRows(8))
            );
        }
    }

}
