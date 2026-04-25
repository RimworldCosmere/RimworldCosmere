using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using Caption = Cosmere.Lightweave.Typography.Typography.Caption;
using Code = Cosmere.Lightweave.Typography.Typography.Code;
using Heading = Cosmere.Lightweave.Typography.Typography.Heading;
using Icon = Cosmere.Lightweave.Typography.Typography.Icon;
using Label = Cosmere.Lightweave.Typography.Typography.Label;
using RichText = Cosmere.Lightweave.Typography.Typography.RichText;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Data;

[Doc(
    Id = "keyvalue",
    Summary = "Single-row label/value pair with a fixed-width label gutter.",
    WhenToUse = "Render a labeled metric, stat, or attribute alongside its value.",
    SourcePath = "CosmereCore/CosmereCore/Lightweave/Data/KeyValue.cs",
    PreferredVariantHeight = 180f
)]
public static class KeyValue {
    public static LightweaveNode Create(
        [DocParam("Label text rendered in the label gutter.")]
        string label,
        [DocParam("Value node rendered to the side of the label.")]
        LightweaveNode value,
        [DocParam("Width of the label gutter. Defaults to 8rem.")]
        Rem? labelWidth = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode labelNode = Text.Create(
            label,
            new FontRef.Role(FontRole.Label)
        );

        LightweaveNode node = NodeBuilder.New("KeyValue", line, file);
        node.Children.Add(labelNode);
        node.Children.Add(value);

        node.Measure = availableWidth => {
            float lw = (labelWidth ?? new Rem(8f)).ToPixels();
            float valueWidth = Mathf.Max(0f, availableWidth - lw);
            float labelH = labelNode.Measure?.Invoke(lw) ?? labelNode.PreferredHeight ?? 0f;
            float valueH = value.Measure?.Invoke(valueWidth) ?? value.PreferredHeight ?? 0f;
            return Mathf.Max(labelH, valueH);
        };

        node.Paint = (rect, paintChildren) => {
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;
            float lw = (labelWidth ?? new Rem(8f)).ToPixels();

            Rect labelRect;
            Rect valueRect;

            if (rtl) {
                labelRect = new Rect(rect.xMax - lw, rect.y, lw, rect.height);
                valueRect = new Rect(rect.x, rect.y, Mathf.Max(0f, rect.width - lw), rect.height);
            } else {
                labelRect = new Rect(rect.x, rect.y, lw, rect.height);
                valueRect = new Rect(rect.x + lw, rect.y, Mathf.Max(0f, rect.width - lw), rect.height);
            }

            labelNode.MeasuredRect = labelRect;
            value.MeasuredRect = valueRect;

            paintChildren();
        };

        return node;
    }

    [DocVariant("CC_Playground_Data_KV_Stormlight")]
    public static DocSample DocsStormlight() {
        LightweaveNode value = Text.Create(
            "1200",
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.SurfaceAccent
        );
        return new DocSample(
            KeyValue.Create((string)"CC_Playground_Data_KV_Stormlight".Translate(), value)
        );
    }

    [DocVariant("CC_Playground_Data_KV_Investiture")]
    public static DocSample DocsInvestiture() {
        LightweaveNode value = Text.Create(
            "Honor",
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextPrimary
        );
        return new DocSample(
            KeyValue.Create((string)"CC_Playground_Data_KV_Investiture".Translate(), value)
        );
    }

    [DocVariant("CC_Playground_Data_KV_SprenBond")]
    public static DocSample DocsSprenBond() {
        LightweaveNode value = Text.Create(
            "Honorspren",
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextPrimary
        );
        return new DocSample(
            KeyValue.Create((string)"CC_Playground_Data_KV_SprenBond".Translate(), value)
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        LightweaveNode value = Text.Create(
            "1200",
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.SurfaceAccent
        );
        return new DocSample(
            KeyValue.Create((string)"CC_Playground_Data_KV_Stormlight".Translate(), value)
        );
    }
}