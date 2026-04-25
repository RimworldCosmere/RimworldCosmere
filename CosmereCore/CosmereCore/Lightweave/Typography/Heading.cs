using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Playground;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Layout.Layout;
using static Cosmere.Lightweave.Playground.PlaygroundChips;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Typography;

public static partial class Typography {
    [Doc(
        Id = "heading",
        Summary = "Bold heading text rendered with the theme heading font.",
        WhenToUse = "Section titles, panel headers, and other prominent labels in a hierarchy.",
        SourcePath = "CosmereCore/CosmereCore/Lightweave/Typography/Heading.cs"
    )]
    public static class Heading {
        public static LightweaveNode Create(
            [DocParam("Heading level. 1 is largest; 4+ falls back to small heading size.")]
            int level,
            [DocParam("Heading text content.")]
            string text,
            [DocParam("Override the heading color. Defaults to TextPrimary via Text().")]
            ColorRef? color = null,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            Rem size = level switch {
                1 => new Rem(2f),
                2 => new Rem(1.5f),
                3 => new Rem(1.25f),
                _ => new Rem(1.125f),
            };
            return Text.Create(text, FontRole.Heading, size, color, TextAlign.Start, FontStyle.Bold, line: line, file: file);
        }

        [DocVariant("CC_Playground_Label_Large")]
        public static DocSample DocsH1() {
            return new DocSample(Heading.Create(1, "Heading 1"));
        }

        [DocVariant("CC_Playground_Label_Medium")]
        public static DocSample DocsH2() {
            return new DocSample(Heading.Create(2, "Heading 2"));
        }

        [DocVariant("CC_Playground_Label_Small")]
        public static DocSample DocsH3() {
            return new DocSample(Heading.Create(3, "Heading 3"));
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(Heading.Create(2, "Surgebinding"));
        }
    }
}
