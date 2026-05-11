using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Feedback;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Doc.DocChips;

namespace Cosmere.Lightweave.Layout;

[Doc(
    Id = "wrap",
    Summary = "Wrapping flow that re-flows children onto new rows when the line fills.",
    WhenToUse = "Variable count of equally-sized chips that should wrap to width.",
    SourcePath = "Lightweave/Lightweave/Layout/Wrap.cs",
    PreferredVariantHeight = 120f,
    ShowRtl = true
)]
public static class Wrap {
    public static LightweaveNode Create(
        [DocParam("Gap between cells.", TypeOverride = "Rem", DefaultOverride = "0")]
        Rem gap = default,
        [DocParam("Minimum width per cell. Drives how many fit per row.", TypeOverride = "Rem", DefaultOverride = "0")]
        Rem minChildWidth = default,
        [DocParam("Builder callback to populate children.")]
        Action<List<LightweaveNode>>? children = null,
        [DocParam("Optional explicit row height. Falls back to minChildWidth * 0.6 when unset.", TypeOverride = "Rem?", DefaultOverride = "null")]
        Rem? lineHeight = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        LightweaveNode node = NodeBuilder.New("Wrap", line, file);
        node.Children.AddRange(kids);

        int FlowCount() {
            int c = 0;
            for (int i = 0; i < kids.Count; i++) {
                if (kids[i].IsInFlow()) {
                    c++;
                }
            }
            return c;
        }

        node.Measure = availableWidth => {
            int flowCount = FlowCount();
            if (flowCount == 0) {
                return 0f;
            }

            float gapPx = gap.ToPixels();
            float minW = Mathf.Max(minChildWidth.ToPixels(), 1f);
            float rowH = lineHeight.HasValue ? lineHeight.Value.ToPixels() : minW * 0.6f;
            int perRow = Mathf.Max(1, Mathf.FloorToInt((availableWidth + gapPx) / (minW + gapPx)));
            int rows = (flowCount + perRow - 1) / perRow;
            return rows * rowH + Mathf.Max(0, rows - 1) * gapPx;
        };

        node.Paint = (rect, paintChildren) => {
            float gapPx = gap.ToPixels();
            float minW = Mathf.Max(minChildWidth.ToPixels(), 1f);
            float rowH = lineHeight.HasValue ? lineHeight.Value.ToPixels() : minW * 0.6f;
            float x = rect.x;
            float y = rect.y;
            foreach (LightweaveNode child in kids) {
                if (!child.IsInFlow()) {
                    continue;
                }
                if (x + minW > rect.xMax) {
                    x = rect.x;
                    y += rowH + gapPx;
                }

                child.MeasuredRect = new Rect(x, y, minW, rowH);
                x += minW + gapPx;
            }

            paintChildren();
        };
        return node;
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => 
            Wrap.Create(
                SpacingScale.Xs,
                new Rem(3f),
                k => {
                    k.Add(SampleChip("one"));
                    k.Add(SampleChip("two"));
                    k.Add(SampleChip("three"));
                    k.Add(SampleChip("four"));
                }
            )
        );
    }

[DocVariant("CL_Playground_Layout_Wrap_Badges", Order = 1)]
    public static DocSample DocsBadges() {
        return new DocSample(() =>
            Wrap.Create(
                SpacingScale.Xs,
                new Rem(7.5f),
                k => {
                    k.Add(Badge.Create("Windrunner", BadgeVariant.Accent));
                    k.Add(Badge.Create("Skybreaker", BadgeVariant.Neutral));
                    k.Add(Badge.Create("Dustbringer", BadgeVariant.Danger));
                    k.Add(Badge.Create("Edgedancer", BadgeVariant.Success));
                    k.Add(Badge.Create("Truthwatcher", BadgeVariant.Accent));
                    k.Add(Badge.Create("Lightweaver", BadgeVariant.Warning));
                    k.Add(Badge.Create("Elsecaller", BadgeVariant.Neutral));
                    k.Add(Badge.Create("Willshaper", BadgeVariant.Neutral));
                    k.Add(Badge.Create("Stoneward", BadgeVariant.Success));
                    k.Add(Badge.Create("Bondsmith", BadgeVariant.Accent));
                    k.Add(Badge.Create("Mistborn", BadgeVariant.Warning));
                    k.Add(Badge.Create("Twinborn", BadgeVariant.Neutral));
                    k.Add(Badge.Create("Coinshot", BadgeVariant.Accent));
                    k.Add(Badge.Create("Lurcher", BadgeVariant.Neutral));
                    k.Add(Badge.Create("Smoker", BadgeVariant.Neutral));
                    k.Add(Badge.Create("Seeker", BadgeVariant.Neutral));
                    k.Add(Badge.Create("Soother", BadgeVariant.Success));
                    k.Add(Badge.Create("Rioter", BadgeVariant.Danger));
                },
                lineHeight: new Rem(1.75f)
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => 
            Wrap.Create(
                SpacingScale.Xs,
                new Rem(3f),
                k => {
                    k.Add(SampleChip("alpha"));
                    k.Add(SampleChip("beta"));
                    k.Add(SampleChip("gamma"));
                }
            )
        );
    }
}
