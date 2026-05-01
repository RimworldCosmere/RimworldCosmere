using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Playground;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using static Cosmere.Lightweave.Layout.Layout;
using static Cosmere.Lightweave.Doc.DocChips;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Typography;

public static partial class Typography {
    [Doc(
        Id = "caption",
        Summary = "Small muted caption text using the Caption font role and TextMuted color.",
        WhenToUse = "Timestamps, secondary metadata, and other low-emphasis descriptors.",
        SourcePath = "CosmereCore/CosmereCore/Lightweave/Typography/Caption.cs"
    )]
    public static class Caption {
        public static LightweaveNode Create(
            [DocParam("Caption text content.")]
            string text,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            return Text.Create(
                text,
                FontRole.Caption,
                new Rem(0.75f),
                ThemeSlot.TextMuted,
                line: line,
                file: file
            );
        }

        [DocVariant("CC_Playground_Label_Default")]
        public static DocSample DocsDefault() {
            return new DocSample(Caption.Create("Updated moments ago"));
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(Caption.Create("Updated moments ago"));
        }
    }
}
