using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cosmere.Lightweave.Typography;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Layout;

public enum KeyValueOrientation {
    Vertical,
    Horizontal,
}

public readonly record struct KeyValueRow(string Label, string Value);

[Doc(
    Id = "key-value-table",
    Summary = "Pairs of small-caps labels with values, stacked vertically or laid out horizontally.",
    WhenToUse = "Metadata blocks, save/mod detail panes, hero card stat rows.",
    SourcePath = "Lightweave/Lightweave/Layout/KeyValueTable.cs",
    ShowRtl = true
)]
public static class KeyValueTable {
    public static LightweaveNode Create(
        [DocParam("Rows of (label, value). Labels are uppercased automatically.")]
        IReadOnlyList<KeyValueRow> rows,
        [DocParam("Vertical = label-left/value-right rows. Horizontal = eyebrow-on-top, value below, cells side by side.")]
        KeyValueOrientation orientation = KeyValueOrientation.Vertical,
        [DocParam("Override label color. Defaults to MetadataLabel slot.")]
        ColorRef? labelColor = null,
        [DocParam("Vertical mode only: fixed width of the label column in rems.")]
        float labelColumnRem = 6f,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        ColorRef resolvedLabel = labelColor ?? (ColorRef)ThemeSlot.MetadataLabel;
        string[]? mergedClasses = StyleExtensions.PrependClass("key-value-table", classes);

        if (orientation == KeyValueOrientation.Horizontal) {
            return HStack.Create(
                gap: SpacingScale.Lg,
                children: h => {
                    for (int i = 0; i < rows.Count; i++) {
                        KeyValueRow row = rows[i];
                        h.AddFlex(BuildHorizontalCell(row.Label, row.Value, resolvedLabel));
                    }
                },
                style: style,
                classes: mergedClasses,
                id: id,
                line: line,
                file: file
            );
        }

        return Stack.Create(
            gap: SpacingScale.Xxs,
            children: s => {
                for (int i = 0; i < rows.Count; i++) {
                    KeyValueRow row = rows[i];
                    s.Add(BuildVerticalRow(row.Label, row.Value, resolvedLabel, labelColumnRem));
                }
            },
            style: style,
            classes: mergedClasses,
            id: id,
            line: line,
            file: file
        );
    }

    private static LightweaveNode BuildHorizontalCell(string label, string value, ColorRef labelColor) {
        return Stack.Create(SpacingScale.Xxs, s => {
            s.Add(Eyebrow.Create(label, style: new Style { TextColor = labelColor }));
            if (!string.IsNullOrEmpty(value)) {
                s.Add(Text.Create(
                    value,
                    style: new Style { FontFamily = FontRole.Body, FontSize = new Rem(0.9375f), TextColor = ThemeSlot.TextPrimary }
                ));
            }
        });
    }

    private static LightweaveNode BuildVerticalRow(string label, string value, ColorRef labelColor, float labelColumnRem) {
        float labelPx = new Rem(labelColumnRem).ToPixels();
        return HStack.Create(SpacingScale.Md, h => {
            h.Add(Eyebrow.Create(label, style: new Style { TextColor = labelColor }), labelPx);
            h.AddFlex(Text.Create(
                value ?? string.Empty,
                style: new Style { FontFamily = FontRole.Mono, FontSize = new Rem(0.9375f), TextColor = ThemeSlot.TextPrimary }
            ));
        });
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsVertical() {
        return new DocSample(() => KeyValueTable.Create(new[] {
            new KeyValueRow("VERSION", "1.6.4525"),
            new KeyValueRow("MODS", "27"),
            new KeyValueRow("LAST PLAYED", "2 hours ago"),
        }));
    }

    [DocVariant("CL_Playground_Label_Horizontal")]
    public static DocSample DocsHorizontal() {
        return new DocSample(() => KeyValueTable.Create(new[] {
            new KeyValueRow("BIOME", "Boreal Forest"),
            new KeyValueRow("CLIMATE", "-14°C"),
            new KeyValueRow("COLONISTS", "6"),
            new KeyValueRow("THREATS", "Manhunter 3h"),
        }, KeyValueOrientation.Horizontal));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => KeyValueTable.Create(new[] {
            new KeyValueRow("VERSION", "1.6.4525"),
            new KeyValueRow("MODS", "27"),
        }));
    }
}
