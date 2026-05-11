using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;

namespace Cosmere.Lightweave.Layout;

[Doc(
    Id = "windowbody",
    Summary = "Body region of a LightweaveWindow with optional padding, fill, and scroll wrapper.",
    WhenToUse = "Override LightweaveWindow.Body() to control body padding/scroll without rebuilding chrome.",
    SourcePath = "Lightweave/Lightweave/Layout/WindowBody.cs"
)]
public static class WindowBody {
    public static LightweaveNode Create(
        Action<List<LightweaveNode>>? children = null,
        bool scrollable = false,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);

        Style baseStyle = new Style {
            Padding = EdgeInsets.All(SpacingScale.Md),
            Background = BackgroundSpec.Of(ThemeSlot.SurfacePrimary),
        };
        Style merged = style.HasValue ? Style.Merge(baseStyle, style.Value) : baseStyle;

        if (!scrollable) {
            return Box.Create(
                c => c.AddRange(kids),
                style: merged,
                classes: StyleExtensions.PrependClass("window-body", classes),
                id: id,
                line: line,
                file: file
            );
        }

        LightweaveNode inner = kids.Count == 1
            ? kids[0]
            : Stack.Create(
                children: s => {
                    for (int i = 0; i < kids.Count; i++) {
                        s.Add(kids[i]);
                    }
                },
                line: line,
                file: file
            );

        return Box.Create(
            c => c.Add(ScrollArea.Create(inner, line: line, file: file)),
            style: merged,
            classes: StyleExtensions.PrependClass("window-body", classes),
            id: id,
            line: line,
            file: file
        );
    }

    
}
