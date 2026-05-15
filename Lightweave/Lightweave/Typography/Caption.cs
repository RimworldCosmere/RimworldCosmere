using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cosmere.Lightweave.Layout;
using RimWorld;
using Verse.AI;
using static Cosmere.Lightweave.Doc.DocChips;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Typography;

public static partial class Typography {
    [Doc(
        Id = "caption",
        Summary = "Small muted caption text using the Caption font role and TextMuted color.",
        WhenToUse = "Timestamps, secondary metadata, and other low-emphasis descriptors.",
        SourcePath = "Lightweave/Lightweave/Typography/Caption.cs",
        ShowRtl = true
    )]
    public static class Caption {
        public static LightweaveNode Create(
            [DocParam("Caption text content.")]
            string text,
            [DocParam("Inline style override.", TypeOverride = "Style?", DefaultOverride = "null")]
            Style? style = null,
            [DocParam("Additional class names merged after the base 'caption' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
            string[]? classes = null,
            [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
            string? id = null,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            return Text.Create(
                text,
                style: style,
                classes: StyleExtensions.PrependClass("caption", classes),
                id: id,
                line: line,
                file: file
            );
        }

        [DocVariant("CL_Playground_Label_Default")]
        public static DocSample DocsDefault() {
            return new DocSample(() => Caption.Create("Updated moments ago"));
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(() => Caption.Create("Updated moments ago"));
        }
    }
}
