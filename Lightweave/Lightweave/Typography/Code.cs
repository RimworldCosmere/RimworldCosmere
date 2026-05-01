using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Playground;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Verse;
using static Cosmere.Lightweave.Layout.Layout;
using static Cosmere.Lightweave.Playground.PlaygroundChips;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Typography;

public static partial class Typography {
    [Doc(
        Id = "code",
        Summary = "Monospaced code text using the Mono font role.",
        WhenToUse = "Inline identifiers, file paths, XML snippets, or any literal source text.",
        SourcePath = "CosmereCore/CosmereCore/Lightweave/Typography/Code.cs"
    )]
    public static class Code {
        public static LightweaveNode Create(
            [DocParam("Code text content. Rendered with the theme Mono font.")]
            string text,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            return Text.Create(
                text,
                FontRole.Mono,
                new Rem(0.875f),
                ThemeSlot.TextPrimary,
                line: line,
                file: file
            );
        }

        [DocVariant("CC_Playground_Label_Inline")]
        public static DocSample DocsInline() {
            return new DocSample(Code.Create("Pawn.health.AddHediff(HediffDef);"));
        }

        [DocVariant("CC_Playground_Label_Xml")]
        public static DocSample DocsXml() {
            return new DocSample(Code.Create("<defName>CC_AbilityWindrunner</defName>"));
        }

        [DocVariant("CC_Playground_Label_Path")]
        public static DocSample DocsPath() {
            return new DocSample(Code.Create("Things/Item/Equipment/Weapon/Shardblade.png"));
        }

        [DocVariant("CC_Playground_Label_Block")]
        public static DocSample DocsBlock() {
            return new DocSample(Code.Create((string)"CC_Playground_Code_Sample".Translate()));
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(Code.Create("Pawn.health.AddHediff(HediffDef);"));
        }
    }
}
