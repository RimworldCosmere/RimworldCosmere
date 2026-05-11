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

[Doc(
    Id = "box",
    Summary = "Padded surface with optional background, border, and radius.",
    WhenToUse = "Wrap content in a themed container with consistent inner padding.",
    SourcePath = "Lightweave/Lightweave/Layout/Box.cs"
)]
public static class Box {
    public static LightweaveNode Create(
        [DocParam("Children appended to the box.")]
        Action<List<LightweaveNode>>? children = null,
        [DocParam("Style applied to the box (padding/background/border/radius/etc).", TypeOverride = "Style?", DefaultOverride = "null")]
        Style? style = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);

        LightweaveNode node = NodeBuilder.New("Box", line, file);
        node.Children.AddRange(kids);
        if (style.HasValue) {
            node.Style = style.Value;
        }

        float ChildMeasure(LightweaveNode child, float width) {
            if (child.Measure != null) {
                return child.Measure(width);
            }

            return child.PreferredHeight ?? 0f;
        }

        (float left, float top, float right, float bottom) ResolvePaddingPixels() {
            Style s = node.GetResolvedStyle();
            EdgeInsets pad = s.Padding ?? EdgeInsets.Zero;
            return pad.Resolve(RenderContext.Current.Direction);
        }

        bool CanMeasure() {
            int n = kids.Count;
            if (n == 0) {
                return true;
            }

            for (int i = 0; i < n; i++) {
                LightweaveNode k = kids[i];
                if (!k.IsInFlow()) {
                    continue;
                }
                if (k.Measure == null && !k.PreferredHeight.HasValue) {
                    return false;
                }
            }

            return true;
        }

        if (CanMeasure()) {
            node.Measure = availableWidth => {
                (float left, float top, float right, float bottom) = ResolvePaddingPixels();
                float innerWidth = Mathf.Max(0f, availableWidth - left - right);
                int n = kids.Count;
                float total = 0f;
                for (int i = 0; i < n; i++) {
                    if (!kids[i].IsInFlow()) {
                        continue;
                    }
                    total += ChildMeasure(kids[i], innerWidth);
                }

                return total + top + bottom;
            };
        }

        node.MeasureWidth = () => {
            (float left, float top, float right, float bottom) = ResolvePaddingPixels();
            float maxW = 0f;
            int n = kids.Count;
            for (int i = 0; i < n; i++) {
                LightweaveNode k = kids[i];
                if (!k.IsInFlow()) {
                    continue;
                }
                float w = k.MeasureWidth?.Invoke() ?? 0f;
                if (w > maxW) {
                    maxW = w;
                }
            }

            return maxW + left + right;
        };

        node.Paint = (rect, paintChildren) => {
            int count = kids.Count;
            if (count == 0) {
                return;
            }

            bool[] inFlow = new bool[count];
            int flowCount = 0;
            for (int i = 0; i < count; i++) {
                inFlow[i] = kids[i].IsInFlow();
                if (inFlow[i]) {
                    flowCount++;
                }
            }

            if (flowCount == 0) {
                paintChildren();
                return;
            }

            bool anyIntrinsic = false;
            float[] intrinsic = new float[count];
            for (int i = 0; i < count; i++) {
                if (!inFlow[i]) {
                    intrinsic[i] = 0f;
                    continue;
                }
                LightweaveNode k = kids[i];
                if (k.Measure != null || k.PreferredHeight.HasValue) {
                    intrinsic[i] = ChildMeasure(k, rect.width);
                    anyIntrinsic = true;
                }
                else {
                    intrinsic[i] = -1f;
                }
            }

            float y = rect.y;
            if (anyIntrinsic) {
                float knownTotal = 0f;
                int unknownCount = 0;
                for (int i = 0; i < count; i++) {
                    if (!inFlow[i]) {
                        continue;
                    }
                    if (intrinsic[i] >= 0f) {
                        knownTotal += intrinsic[i];
                    }
                    else {
                        unknownCount++;
                    }
                }

                float remaining = Mathf.Max(0f, rect.height - knownTotal);
                float unknownEach = unknownCount > 0 ? remaining / unknownCount : 0f;
                for (int i = 0; i < count; i++) {
                    if (!inFlow[i]) {
                        continue;
                    }
                    LightweaveNode child = kids[i];
                    float h = intrinsic[i] >= 0f ? intrinsic[i] : unknownEach;
                    child.MeasuredRect = new Rect(rect.x, y, rect.width, h);
                    y += h;
                }
            }
            else {
                float eachH = rect.height / flowCount;
                for (int i = 0; i < count; i++) {
                    if (!inFlow[i]) {
                        continue;
                    }
                    LightweaveNode child = kids[i];
                    child.MeasuredRect = new Rect(rect.x, y, rect.width, eachH);
                    y += eachH;
                }
            }

            paintChildren();
        };
        return node;
    }

    [DocVariant("CL_Playground_Label_Raised")]
    public static DocSample DocsRaised() {
        return new DocSample(() => 
            Box.Create(
                c => c.Add(Caption.Create("raised")),
                style: new Style {
                    Padding = EdgeInsets.All(SpacingScale.Sm),
                    Background = BackgroundSpec.Of(ThemeSlot.SurfaceRaised),
                    Border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault),
                    Radius = RadiusSpec.All(RadiusScale.Sm),
                }
            )
        );
    }

    [DocVariant("CL_Playground_Label_Sunken")]
    public static DocSample DocsSunken() {
        return new DocSample(() => 
            Box.Create(
                c => c.Add(Caption.Create("sunken")),
                style: new Style {
                    Padding = EdgeInsets.All(SpacingScale.Sm),
                    Background = BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
                    Radius = RadiusSpec.All(RadiusScale.Sm),
                }
            )
        );
    }

    [DocVariant("CL_Playground_Label_Accent")]
    public static DocSample DocsAccent() {
        return new DocSample(() => 
            Box.Create(
                c => c.Add(Text.Create("accent", FontRole.Body, new Rem(0.8125f), ThemeSlot.TextOnAccent)),
                style: new Style {
                    Padding = EdgeInsets.All(SpacingScale.Sm),
                    Background = BackgroundSpec.Of(ThemeSlot.SurfaceAccent),
                    Radius = RadiusSpec.All(RadiusScale.Sm),
                }
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => 
            Box.Create(
                c => c.Add(Caption.Create("content")),
                style: new Style {
                    Padding = EdgeInsets.All(SpacingScale.Sm),
                    Background = BackgroundSpec.Of(ThemeSlot.SurfaceRaised),
                    Border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault),
                    Radius = RadiusSpec.All(RadiusScale.Sm),
                }
            )
        );
    }
}
