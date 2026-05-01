using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Layout;

public static partial class Layout {
    [Doc(
        Id = "box",
        Summary = "Padded surface with optional background, border, and radius.",
        WhenToUse = "Wrap content in a themed container with consistent inner padding.",
        SourcePath = "CosmereCore/CosmereCore/Lightweave/Layout/Box.cs"
    )]
    public static class Box {
        public static LightweaveNode Create(
            [DocParam("Inner padding around the children.", TypeOverride = "EdgeInsets?", DefaultOverride = "null")]
            EdgeInsets? padding = null,
            [DocParam("Background fill spec.", TypeOverride = "BackgroundSpec?", DefaultOverride = "null")]
            BackgroundSpec? background = null,
            [DocParam("Border spec for outline.", TypeOverride = "BorderSpec?", DefaultOverride = "null")]
            BorderSpec? border = null,
            [DocParam("Corner radius spec.", TypeOverride = "RadiusSpec?", DefaultOverride = "null")]
            RadiusSpec? radius = null,
            [DocParam("Children appended to the box.")]
            Action<List<LightweaveNode>>? children = null,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            List<LightweaveNode> kids = new List<LightweaveNode>();
            children?.Invoke(kids);
            EdgeInsets pad = padding ?? EdgeInsets.Zero;

            LightweaveNode node = NodeBuilder.New("Box", line, file);
            node.Children.AddRange(kids);

            float ChildMeasure(LightweaveNode child, float width) {
                if (child.Measure != null) {
                    return child.Measure(width);
                }

                return child.PreferredHeight ?? 0f;
            }

            bool CanMeasure() {
                int n = kids.Count;
                if (n == 0) {
                    return true;
                }

                for (int i = 0; i < n; i++) {
                    LightweaveNode k = kids[i];
                    if (k.Measure == null && !k.PreferredHeight.HasValue) {
                        return false;
                    }
                }

                return true;
            }

            if (CanMeasure()) {
                node.Measure = availableWidth => {
                    Direction dir = RenderContext.Current.Direction;
                    (float left, float top, float right, float bottom) = pad.Resolve(dir);
                    float innerWidth = Mathf.Max(0f, availableWidth - left - right);
                    int n = kids.Count;
                    float total = 0f;
                    for (int i = 0; i < n; i++) {
                        total += ChildMeasure(kids[i], innerWidth);
                    }

                    return total + top + bottom;
                };
            }

            node.Paint = (rect, paintChildren) => {
                PaintBox.Draw(rect, background, border, radius);
                Rect content = pad.Shrink(rect, RenderContext.Current.Direction);
                int count = kids.Count;
                if (count == 0) {
                    return;
                }

                bool anyIntrinsic = false;
                float[] intrinsic = new float[count];
                for (int i = 0; i < count; i++) {
                    LightweaveNode k = kids[i];
                    if (k.Measure != null || k.PreferredHeight.HasValue) {
                        intrinsic[i] = ChildMeasure(k, content.width);
                        anyIntrinsic = true;
                    } else {
                        intrinsic[i] = -1f;
                    }
                }

                float y = content.y;
                if (anyIntrinsic) {
                    float knownTotal = 0f;
                    int unknownCount = 0;
                    for (int i = 0; i < count; i++) {
                        if (intrinsic[i] >= 0f) {
                            knownTotal += intrinsic[i];
                        } else {
                            unknownCount++;
                        }
                    }

                    float remaining = Mathf.Max(0f, content.height - knownTotal);
                    float unknownEach = unknownCount > 0 ? remaining / unknownCount : 0f;
                    for (int i = 0; i < count; i++) {
                        LightweaveNode child = kids[i];
                        float h = intrinsic[i] >= 0f ? intrinsic[i] : unknownEach;
                        child.MeasuredRect = new Rect(content.x, y, content.width, h);
                        y += h;
                    }
                } else {
                    float eachH = content.height / count;
                    for (int i = 0; i < count; i++) {
                        LightweaveNode child = kids[i];
                        child.MeasuredRect = new Rect(content.x, y, content.width, eachH);
                        y += eachH;
                    }
                }

                paintChildren();
            };
            return node;
        }

        [DocVariant("CC_Playground_Label_Raised")]
        public static DocSample DocsRaised() {
            return new DocSample(
                Box.Create(
                    EdgeInsets.All(SpacingScale.Sm),
                    new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised),
                    BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault),
                    RadiusSpec.All(new Rem(0.25f)),
                    c => c.Add(Caption.Create("raised"))
                )
            );
        }

        [DocVariant("CC_Playground_Label_Sunken")]
        public static DocSample DocsSunken() {
            return new DocSample(
                Box.Create(
                    EdgeInsets.All(SpacingScale.Sm),
                    new BackgroundSpec.Solid(ThemeSlot.SurfaceSunken),
                    null,
                    RadiusSpec.All(new Rem(0.25f)),
                    c => c.Add(Caption.Create("sunken"))
                )
            );
        }

        [DocVariant("CC_Playground_Label_Accent")]
        public static DocSample DocsAccent() {
            return new DocSample(
                Box.Create(
                    EdgeInsets.All(SpacingScale.Sm),
                    new BackgroundSpec.Solid(ThemeSlot.SurfaceAccent),
                    null,
                    RadiusSpec.All(new Rem(0.25f)),
                    c => c.Add(Text.Create("accent", FontRole.Body, new Rem(0.8125f), ThemeSlot.TextOnAccent))
                )
            );
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(
                Box.Create(
                    EdgeInsets.All(SpacingScale.Sm),
                    new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised),
                    BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault),
                    RadiusSpec.All(new Rem(0.25f)),
                    c => c.Add(Caption.Create("content"))
                )
            );
        }
    }
}
