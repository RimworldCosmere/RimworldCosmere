using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Layout.Layout;
using Caption = Cosmere.Lightweave.Typography.Typography.Caption;
using Code = Cosmere.Lightweave.Typography.Typography.Code;
using Heading = Cosmere.Lightweave.Typography.Typography.Heading;
using Icon = Cosmere.Lightweave.Typography.Typography.Icon;
using Label = Cosmere.Lightweave.Typography.Typography.Label;
using RichText = Cosmere.Lightweave.Typography.Typography.RichText;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Data;

[Doc(
    Id = "list",
    Summary = "Vertical scrolling list of items rendered through a row builder.",
    WhenToUse = "Show a homogenous, potentially long sequence of rows that benefits from virtualization.",
    SourcePath = "CosmereCore/CosmereCore/Lightweave/Data/List.cs",
    PreferredVariantHeight = 200f
)]
public static class List {
    public static LightweaveNode Create<T>(
        [DocParam("Source items rendered top-to-bottom.")]
        IReadOnlyList<T> items,
        [DocParam("Builds the row node for each item, given the item and its index.")]
        Func<T, int, LightweaveNode> rowBuilder,
        [DocParam("Fixed row height in pixels. Required for virtualization.")]
        float? rowHeight = null,
        [DocParam("Stable key extractor used to preserve row identity across renders.")]
        Func<T, object>? keyFn = null,
        [DocParam("When true, only rows in view are built. Requires rowHeight.")]
        bool virtualize = true,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Hooks.Hooks.RefHandle<LightweaveScrollStatus> statusRef =
            Hooks.Hooks.UseRef(new LightweaveScrollStatus(), line, file);

        LightweaveNode node = NodeBuilder.New($"List<{typeof(T).Name}>", line, file);

        if (rowHeight.HasValue && items != null) {
            node.PreferredHeight = items.Count * rowHeight.Value;
        }

        node.Paint = (rect, paintChildren) => {
            if (items == null) {
                return;
            }

            bool doVirtualize = virtualize && rowHeight.HasValue;
            float totalHeight = rowHeight.HasValue
                ? items.Count * rowHeight.Value
                : items.Count * 36f;

            statusRef.Current.Height = totalHeight;
            using (new LightweaveScrollView(rect, statusRef.Current)) {
                float scrollbarGutter = LightweaveScrollView.GutterPixels(statusRef.Current.VerticalVisible);
                float innerWidth = rect.width - scrollbarGutter;

                node.Children.Clear();

                if (doVirtualize) {
                    float rh = Mathf.Max(1f, rowHeight!.Value);
                    float scrollY = statusRef.Current.Position.y;

                    int startIdx = Math.Max(0, (int)Math.Floor(scrollY / rh) - 2);
                    int endIdx = Math.Min(items.Count, (int)Math.Ceiling((scrollY + rect.height) / rh) + 2);

                    for (int i = startIdx; i < endIdx; i++) {
                        LightweaveNode row = rowBuilder(items[i], i);
                        row.ExplicitKey = keyFn?.Invoke(items[i]) ?? i;
                        row.MeasuredRect = new Rect(0f, i * rh, innerWidth, rh);
                        node.Children.Add(row);
                    }
                } else {
                    float rh = rowHeight ?? 36f;
                    for (int i = 0; i < items.Count; i++) {
                        LightweaveNode row = rowBuilder(items[i], i);
                        row.ExplicitKey = keyFn?.Invoke(items[i]) ?? i;
                        row.MeasuredRect = new Rect(0f, i * rh, innerWidth, rh);
                        node.Children.Add(row);
                    }
                }

                paintChildren();
            }
        };

        return node;
    }

    private static string[] BuildSampleItems() {
        return new[] {
            (string)"CC_Playground_DemoItem_Highstorm".Translate(),
            (string)"CC_Playground_DemoItem_Stormlight".Translate(),
            (string)"CC_Playground_DemoItem_Radiant".Translate(),
            (string)"CC_Playground_DemoItem_Emotion".Translate(),
            (string)"CC_Playground_DemoItem_Alpha".Translate(),
            (string)"CC_Playground_DemoItem_Beta".Translate(),
            (string)"CC_Playground_DemoItem_Gamma".Translate(),
            (string)"CC_Playground_DemoItem_Delta".Translate(),
        };
    }

    private static LightweaveNode BuildSampleList() {
        string[] items = BuildSampleItems();
        return List.Create(
            items,
            (item, _) => Box.Create(
                new EdgeInsets(SpacingScale.Sm, Bottom: SpacingScale.Sm, Left: SpacingScale.Md, Right: SpacingScale.Md),
                null,
                null,
                null,
                k => k.Add(
                    Text.Create(
                        item,
                        FontRole.Body,
                        new Rem(0.9375f),
                        ThemeSlot.TextPrimary
                    )
                )
            ),
            new Rem(2.25f).ToPixels()
        );
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(BuildSampleList());
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(BuildSampleList());
    }
}