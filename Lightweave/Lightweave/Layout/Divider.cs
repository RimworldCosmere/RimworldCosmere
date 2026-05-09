using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Layout;

[Doc(
    Id = "divider",
    Summary = "Thin separator drawn at theme border color.",
    WhenToUse = "Subdivide a Stack/Row visually without adding extra background.",
    SourcePath = "Lightweave/Lightweave/Layout/Divider.cs"
)]
public static class Divider {
    public static LightweaveNode Horizontal(
        [DocParam("Optional thickness. Defaults to 1/16 rem.", TypeOverride = "Rem?", DefaultOverride = "1/16 rem")]
        Rem? thickness = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        float t = (thickness ?? new Rem(1f / 16f)).ToPixels();
        LightweaveNode n = NodeBuilder.New("Divider.Horizontal", line, file);
        n.PreferredHeight = t;
        n.Paint = (rect, _) => {
            Rect bar = new Rect(rect.x, rect.y + (rect.height - t) / 2f, rect.width, t);
            PaintBox.Draw(bar, BackgroundSpec.Of(ThemeSlot.BorderSubtle), null, null);
        };
        return n;
    }

    public static LightweaveNode Vertical(
        [DocParam("Optional thickness. Defaults to 1/16 rem.", TypeOverride = "Rem?", DefaultOverride = "1/16 rem")]
        Rem? thickness = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        float t = (thickness ?? new Rem(1f / 16f)).ToPixels();
        LightweaveNode n = NodeBuilder.New("Divider.Vertical", line, file);
        n.Paint = (rect, _) => {
            Rect bar = new Rect(rect.x + (rect.width - t) / 2f, rect.y, t, rect.height);
            PaintBox.Draw(bar, BackgroundSpec.Of(ThemeSlot.BorderSubtle), null, null);
        };
        return n;
    }

    [DocVariant("CL_Playground_Label_Horizontal")]
    public static DocSample DocsHorizontal() {
        return new DocSample(() => 
            Stack.Create(
                SpacingScale.Xxs,
                s => {
                    s.Add(Caption.Create("above"), 14f);
                    s.Add(Divider.Horizontal(), 2f);
                    s.Add(Caption.Create("below"), 14f);
                }
            )
        );
    }

    [DocVariant("CL_Playground_Label_Vertical")]
    public static DocSample DocsVertical() {
        return new DocSample(() => 
            Row.Create(
                SpacingScale.Xs,
                children: r => {
                    r.Add(Caption.Create("left"));
                    r.Add(Divider.Vertical());
                    r.Add(Caption.Create("right"));
                }
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => 
            Stack.Create(
                SpacingScale.Xxs,
                s => {
                    s.Add(Caption.Create("above"), 14f);
                    s.Add(Divider.Horizontal(), 2f);
                    s.Add(Caption.Create("below"), 14f);
                }
            )
        );
    }
}
