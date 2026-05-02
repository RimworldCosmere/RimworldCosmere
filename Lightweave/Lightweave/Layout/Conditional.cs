using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using static Cosmere.Lightweave.Doc.DocChips;
using Verse;

namespace Cosmere.Lightweave.Layout;

public static partial class Layout {
    [Doc(
        Id = "conditional",
        Summary = "Renders children only when a condition is true.",
        WhenToUse = "Skip a subtree without occupying space when a flag is false.",
        SourcePath = "Lightweave/Lightweave/Layout/Conditional.cs"
    )]
    public static class Conditional {
        public static LightweaveNode When(
            [DocParam("Condition that gates the subtree.")]
            bool when,
            [DocParam("Factory invoked only when condition is true.")]
            Func<LightweaveNode> children,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            LightweaveNode n = NodeBuilder.New($"Conditional({when})", line, file);
            if (when) {
                LightweaveNode child = children();
                n.Children.Add(child);
                if (child.Measure != null) {
                    n.Measure = child.Measure;
                } else if (child.PreferredHeight.HasValue) {
                    n.PreferredHeight = child.PreferredHeight.Value;
                }

                n.Paint = (rect, paintChildren) => {
                    child.MeasuredRect = rect;
                    paintChildren();
                };
            } else {
                n.PreferredHeight = 0f;
                n.Paint = (_, _) => { };
            }

            return n;
        }

        [DocVariant("CC_Playground_Label_True")]
        public static DocSample DocsTrue() {
            return new DocSample(
                Conditional.When(
                    true,
                    () => AccentChip((string)"CC_Playground_Conditional_On".Translate())
                )
            );
        }

        [DocVariant("CC_Playground_Label_False")]
        public static DocSample DocsFalse() {
            return new DocSample(
                Box.Create(
                    EdgeInsets.All(SpacingScale.Xs),
                    new BackgroundSpec.Solid(ThemeSlot.SurfaceSunken),
                    null,
                    RadiusSpec.All(new Rem(0.25f)),
                    k => {
                        k.Add(MutedChip("subtree skipped — no draw"));
                        k.Add(
                            Conditional.When(
                                false,
                                () => AccentChip("hidden")
                            )
                        );
                    }
                )
            );
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(
                Conditional.When(
                    true,
                    () => AccentChip((string)"CC_Playground_Conditional_On".Translate())
                )
            );
        }
    }
}
