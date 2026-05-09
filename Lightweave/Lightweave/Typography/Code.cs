using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Verse;

namespace Cosmere.Lightweave.Typography;

public static partial class Typography {
    [Doc(
        Id = "code",
        Summary = "Monospaced code text using the Mono font role.",
        WhenToUse = "Inline identifiers, file paths, XML snippets, or any literal source text.",
        SourcePath = "Lightweave/Lightweave/Typography/Code.cs"
    )]
    public static class Code {
        public static LightweaveNode Create(
            [DocParam("Code text content. Rendered with the theme Mono font.")]
            string text,
            [DocParam(
                "If true, renders as a rich block (line numbers, sunken background, copy button, syntax highlighting). Defaults to true if text contains newlines."
            )]
            bool? block = null,
            [DocParam("If true, hides the border and outer rounding so the block can embed inside another surface.")]
            bool flat = false,
            [DocParam("If true and the block exceeds the collapsed line count, shows a 'view code' expander.")]
            bool collapsible = true,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            bool renderBlock = block ?? text.Contains('\n');
            if (renderBlock) {
                return Doc.Doc.CodeBlock(text, flat, collapsible, line: line, file: file);
            }

            return Text.Create(
                text,
                FontRole.Mono,
                new Rem(0.875f),
                ThemeSlot.TextPrimary,
                line: line,
                file: file
            );
        }

        [DocVariant("CL_Playground_Label_Inline")]
        public static DocSample DocsInline() {
            return new DocSample(() => Create("Pawn.health.AddHediff(HediffDef);"));
        }

        [DocVariant("CL_Playground_Label_Xml")]
        public static DocSample DocsXml() {
            return new DocSample(() => Create("<defName>CC_AbilityWindrunner</defName>"));
        }

        [DocVariant("CL_Playground_Label_Path")]
        public static DocSample DocsPath() {
            return new DocSample(() => Create("Things/Item/Equipment/Weapon/Shardblade.png"));
        }

        [DocVariant("CL_Playground_Label_Block")]
        public static DocSample DocsBlock() {
            return new DocSample(() => Create((string)"CL_Playground_Code_Sample".Translate(), true));
        }

        [DocVariant("CL_Playground_Typography_Code_Complex", Order = 5)]
        public static DocSample DocsComplex() {
            return new DocSample(() => Create(
                    "using Cosmere.System.Roshar.Surgebinding;\n" +
                    "using Verse;\n" +
                    "\n" +
                    "public static class WindrunnerCast {\n" +
                    "    public static bool TryFullLashing(Pawn caster, IntVec3 target) {\n" +
                    "        StormlightReserve reserve = caster.Stormlight();\n" +
                    "        if (reserve.Charges < 1) {\n" +
                    "            return false;\n" +
                    "        }\n" +
                    "\n" +
                    "        reserve.Spend(1);\n" +
                    "        Surge gravitation = caster.SurgeOf(SurgeDef.Gravitation);\n" +
                    "        gravitation.LashCellDownward(target, magnitude: 1f);\n" +
                    "\n" +
                    "        Find.LetterStack.ReceiveLetter(\n" +
                    "            \"CRO_Letter_FullLashing\".Translate(caster.Named(\"PAWN\")),\n" +
                    "            \"CRO_Letter_FullLashing_Desc\".Translate(),\n" +
                    "            LetterDefOf.NeutralEvent\n" +
                    "        );\n" +
                    "        return true;\n" +
                    "    }\n" +
                    "}",
                    true,
                    collapsible: false
                )
            );
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(() => Create("Pawn.health.AddHediff(HediffDef);"));
        }
    }
}