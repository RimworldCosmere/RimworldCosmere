using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using static Cosmere.Lightweave.Doc.DocChips;

namespace Cosmere.Lightweave.Layout;

[Doc(
    Id = "spacer",
    Summary = "Empty layout node that consumes space.",
    WhenToUse = "Push siblings apart in a Row/HStack/Column.",
    SourcePath = "Lightweave/Lightweave/Layout/Spacer.cs"
)]
public static class Spacer {
    public static LightweaveNode Flex(
        [DocParam("Relative share of leftover space.")]
        int weight = 1,
        [DocParam("Inline style override.", TypeOverride = "Style?", DefaultOverride = "null")]
        Style? style = null,
        [DocParam("Additional class names merged after the base 'spacer' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
        string[]? classes = null,
        [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode n = NodeBuilder.New($"Spacer.Flex({weight})", line, file);
        n.ApplyStyling("spacer", style, classes, id);
        n.Paint = (_, _) => { };
        return n;
    }

    public static LightweaveNode Fixed(
        [DocParam("Fixed size in Rem units.")]
        Rem size,
        [DocParam("Inline style override.", TypeOverride = "Style?", DefaultOverride = "null")]
        Style? style = null,
        [DocParam("Additional class names merged after the base 'spacer' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
        string[]? classes = null,
        [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode n = NodeBuilder.New($"Spacer.Fixed({size.Value})", line, file);
        n.ApplyStyling("spacer", style, classes, id);
        n.PreferredHeight = size.ToPixels();
        n.Paint = (_, _) => { };
        return n;
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsFlex() {
        return new DocSample(() => 
            HStack.Create(
                SpacingScale.Xs,
                r => {
                    r.Add(SampleChip("start"), 48f);
                    r.AddFlex(Spacer.Flex());
                    r.Add(SampleChip("end"), 48f);
                }
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => 
            HStack.Create(
                SpacingScale.Xs,
                r => {
                    r.Add(SampleChip("left"), 48f);
                    r.AddFlex(Spacer.Flex());
                    r.Add(SampleChip("right"), 48f);
                }
            )
        );
    }
}
