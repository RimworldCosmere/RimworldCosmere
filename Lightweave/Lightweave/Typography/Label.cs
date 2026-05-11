using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cosmere.Lightweave.Layout;
using static Cosmere.Lightweave.Doc.DocChips;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Typography;

public static partial class Typography {
    [Doc(
        Id = "label",
        Summary = "Compact label text using the Label font role and TextSecondary color.",
        WhenToUse = "Form field labels, toolbar captions, and other compact descriptors.",
        SourcePath = "Lightweave/Lightweave/Typography/Label.cs",
        ShowRtl = true
    )]
    public static class Label {
        public static LightweaveNode Create(
            [DocParam("Label text content.")]
            string text,
            [DocParam("Inline style override.", TypeOverride = "Style?", DefaultOverride = "null")]
            Style? style = null,
            [DocParam("Additional class names merged after the base 'label' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
            string[]? classes = null,
            [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
            string? id = null,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            return Text.Create(
                text,
                style: style,
                classes: StyleExtensions.PrependClass("label", classes),
                id: id,
                line: line,
                file: file
            );
        }

        [DocVariant("CL_Playground_Label_Default")]
        public static DocSample DocsDefault() {
            return new DocSample(() => Label.Create("Storm warnings"));
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(() => Label.Create("Storm warnings"));
        }
    }
}
