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
        [DocParam("Body children appended inside the padded surface.")]
        Action<List<LightweaveNode>>? children = null,
        [DocParam("Inner padding around the body content.", TypeOverride = "EdgeInsets?", DefaultOverride = "null")]
        EdgeInsets? padding = null,
        [DocParam("Theme slot used for the body background. Pass null for transparent.", TypeOverride = "ThemeSlot?", DefaultOverride = "SurfacePrimary")]
        ThemeSlot? backgroundSlot = ThemeSlot.SurfacePrimary,
        [DocParam("Wrap children in a ScrollArea so the body becomes scrollable when content overflows.")]
        bool scrollable = false,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);

        BackgroundSpec? bg = backgroundSlot.HasValue
            ? BackgroundSpec.Of(backgroundSlot.Value)
            : null;

        if (!scrollable) {
            return Box.Create(
                c => c.AddRange(kids),
                style: new Style {
                    Padding = padding,
                    Background = bg,
                },
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
            style: new Style {
                Padding = padding,
                Background = bg,
            },
            line: line,
            file: file
        );
    }
}
