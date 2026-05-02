using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using static Cosmere.Lightweave.Layout.Layout;
using static Cosmere.Lightweave.Doc.DocChips;

namespace Cosmere.Lightweave.Layout;

public static partial class Layout {
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
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            LightweaveNode n = NodeBuilder.New($"Spacer.Flex({weight})", line, file);
            n.Paint = (_, _) => { };
            return n;
        }

        public static LightweaveNode Fixed(
            [DocParam("Fixed size in Rem units.")]
            Rem size,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            LightweaveNode n = NodeBuilder.New($"Spacer.Fixed({size.Value})", line, file);
            n.PreferredHeight = size.ToPixels();
            n.Paint = (_, _) => { };
            return n;
        }

        [DocVariant("CC_Playground_Label_Default")]
        public static DocSample DocsFlex() {
            return new DocSample(
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
            return new DocSample(
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
}