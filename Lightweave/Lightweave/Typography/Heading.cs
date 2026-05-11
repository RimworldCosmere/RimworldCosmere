using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Cosmere.Lightweave.Layout;
using static Cosmere.Lightweave.Doc.DocChips;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Typography;

public static partial class Typography {
    [Doc(
        Id = "heading",
        Summary = "Bold heading text rendered with the theme heading font.",
        WhenToUse = "Section titles, panel headers, and other prominent labels in a hierarchy.",
        SourcePath = "Lightweave/Lightweave/Typography/Heading.cs",
        ShowRtl = true
    )]
    public static class Heading {
        public static LightweaveNode Create(
            [DocParam("Heading level. 1 is largest; 4+ falls back to small heading size.")]
            int level,
            [DocParam("Heading text content.")]
            string text,
            [DocParam("Inline style override.", TypeOverride = "Style?", DefaultOverride = "null")]
            Style? style = null,
            [DocParam("Additional class names merged after the base 'heading'/'h{level}' classes.", TypeOverride = "string[]?", DefaultOverride = "null")]
            string[]? classes = null,
            [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
            string? id = null,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            string sizeClass = level switch {
                1 => "h1",
                2 => "h2",
                3 => "h3",
                _ => "h4",
            };
            string[] basedClasses = classes == null
                ? new[] { "heading", sizeClass }
                : ConcatClasses(new[] { "heading", sizeClass }, classes);
            return Text.Create(text, style: style, classes: basedClasses, id: id, line: line, file: file);
        }

        private static string[] ConcatClasses(string[] head, string[] tail) {
            string[] result = new string[head.Length + tail.Length];
            Array.Copy(head, 0, result, 0, head.Length);
            Array.Copy(tail, 0, result, head.Length, tail.Length);
            return result;
        }

        [DocVariant("CL_Playground_Label_Large")]
        public static DocSample DocsH1() {
            return new DocSample(() => Heading.Create(1, "Heading 1"));
        }

        [DocVariant("CL_Playground_Label_Medium")]
        public static DocSample DocsH2() {
            return new DocSample(() => Heading.Create(2, "Heading 2"));
        }

        [DocVariant("CL_Playground_Label_Small")]
        public static DocSample DocsH3() {
            return new DocSample(() => Heading.Create(3, "Heading 3"));
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(() => Heading.Create(2, "Surgebinding"));
        }
    }
}
