using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Doc.DocChips;

namespace Cosmere.Lightweave.Layout;

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
        [DocParam("Scrollbar visibility mode. Auto: hidden until scroll/hover; Always: visible; Never: hidden.")]
        ScrollbarMode scrollbarMode = ScrollbarMode.Auto,
        [DocParam("Scrollbar visual style. Slim: thin overlay; Standard: full-width with arrows; Minimal: thinnest, no track.")]
        ScrollbarStyle scrollbarStyle = ScrollbarStyle.Slim,
        [DocParam("Inline style override.", TypeOverride = "Style?", DefaultOverride = "null")]
        Style? style = null,
        [DocParam("Additional class names merged after the base 'scroll-area' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
        string[]? classes = null,
        [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
        string? id = null,
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

        ScrollbarMode effectiveMode = showScrollbar ? scrollbarMode : ScrollbarMode.Never;
        LightweaveNode node = BuildScrollArea(content, statusRef.Current, effectiveMode, scrollbarStyle, line, file);
        node.ApplyStyling("scroll-area", style, classes, id);
        return node;
    }

    public static LightweaveNode External(
        LightweaveNode content,
        LightweaveScrollStatus status,
        bool showScrollbar = true,
        ScrollbarMode scrollbarMode = ScrollbarMode.Auto,
        ScrollbarStyle scrollbarStyle = ScrollbarStyle.Slim,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        ScrollbarMode effectiveMode = showScrollbar ? scrollbarMode : ScrollbarMode.Never;
        LightweaveNode node = BuildScrollArea(content, status, effectiveMode, scrollbarStyle, line, file);
        node.ApplyStyling("scroll-area", style, classes, id);
        return node;
    }

    private static LightweaveNode BuildScrollArea(
        LightweaveNode content,
        LightweaveScrollStatus status,
        ScrollbarMode mode,
        ScrollbarStyle style,
        int line,
        string file
    ) {
        LightweaveNode node = NodeBuilder.New("ScrollArea", line, file);
        node.Children.Add(content);
        node.Paint = (rect, paintChildren) => {
            float scrollbarGutter = LightweaveScrollView.GutterPixels(status.VerticalVisible, mode, style);
            float innerWidth = rect.width - scrollbarGutter;
            float contentHeight = content.IsInFlow()
                ? content.Measure?.Invoke(innerWidth) ?? content.PreferredHeight ?? rect.height
                : rect.height;
            status.Height = contentHeight;
            using (new LightweaveScrollView(rect, status, mode, style)) {
                if (content.IsInFlow()) {
                    content.MeasuredRect = new Rect(0f, 0f, innerWidth, contentHeight);
                }
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

    [DocVariant("CL_Playground_ScrollArea_AutoSlim")]
    public static DocSample DocsAutoSlim() {
        return new DocSample(() =>
            ScrollArea.Create(DocsRows(20))
        );
    }

    [DocVariant("CL_Playground_ScrollArea_AutoMinimal")]
    public static DocSample DocsAutoMinimal() {
        return new DocSample(() =>
            ScrollArea.Create(DocsRows(20), scrollbarStyle: ScrollbarStyle.Minimal)
        );
    }

    [DocVariant("CL_Playground_ScrollArea_AlwaysSlim")]
    public static DocSample DocsAlwaysSlim() {
        return new DocSample(() =>
            ScrollArea.Create(DocsRows(20), scrollbarMode: ScrollbarMode.Always)
        );
    }

    [DocVariant("CL_Playground_ScrollArea_AlwaysStandard")]
    public static DocSample DocsAlwaysStandard() {
        return new DocSample(() =>
            ScrollArea.Create(
                DocsRows(20),
                scrollbarMode: ScrollbarMode.Always,
                scrollbarStyle: ScrollbarStyle.Standard
            )
        );
    }

    [DocVariant("CL_Playground_ScrollArea_NoBar")]
    public static DocSample DocsNoBar() {
        return new DocSample(() =>
            ScrollArea.Create(DocsRows(20), showScrollbar: false)
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => 
            ScrollArea.Create(DocsRows(8))
        );
    }
}
