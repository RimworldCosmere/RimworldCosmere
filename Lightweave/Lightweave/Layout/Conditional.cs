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
        [DocParam("Inline style override.", TypeOverride = "Style?", DefaultOverride = "null")]
        Style? style = null,
        [DocParam("Additional class names merged after the base 'conditional' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
        string[]? classes = null,
        [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode n = NodeBuilder.New($"Conditional({when})", line, file);
        n.ApplyStyling("conditional", style, classes, id);
        if (when) {
            LightweaveNode child = children();
            n.Children.Add(child);
            if (child.Measure != null) {
                n.Measure = child.Measure;
            }
            else if (child.PreferredHeight.HasValue) {
                n.PreferredHeight = child.PreferredHeight.Value;
            }

            n.Paint = (rect, paintChildren) => {
                child.MeasuredRect = rect;
                paintChildren();
            };
        }
        else {
            n.PreferredHeight = 0f;
            n.Paint = (_, _) => { };
        }

        return n;
    }

    [DocVariant("CL_Playground_Label_True")]
    public static DocSample DocsTrue() {
        return new DocSample(() => 
            Conditional.When(
                true,
                () => AccentChip((string)"CL_Playground_Conditional_On".Translate())
            )
        );
    }

    [DocVariant("CL_Playground_Label_False")]
    public static DocSample DocsFalse() {
        return new DocSample(() => 
            Box.Create(
                k => {
                    k.Add(MutedChip("subtree skipped — no draw"));
                    k.Add(
                        Conditional.When(
                            false,
                            () => AccentChip("hidden")
                        )
                    );
                },
                style: new Style {
                    Padding = EdgeInsets.All(SpacingScale.Xs),
                    Background = BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
                    Radius = RadiusSpec.All(RadiusScale.Sm),
                }
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => 
            Conditional.When(
                true,
                () => AccentChip((string)"CL_Playground_Conditional_On".Translate())
            )
        );
    }
}
