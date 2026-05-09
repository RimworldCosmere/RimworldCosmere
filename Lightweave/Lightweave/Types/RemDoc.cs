using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Theme;
using Cosmere.Lightweave.Tokens;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Types;

[Doc(
    Id = "rem",
    Summary = "Sizing primitive used for every spacing, padding, font size, and border thickness in Lightweave.",
    WhenToUse = "Pass Rem (or a SpacingScale step) anywhere a size is needed so the whole token system scales together when base units change.",
    SourcePath = "Lightweave/Lightweave/Types/Rem.cs",
    Category = "Tokens",
    PreferredVariantHeight = 160f
)]
public static class RemDoc {
    private static readonly (string Label, Rem Step)[] LadderSteps = {
        ("Xxs (0.25)", SpacingScale.Xxs),
        ("Xs (0.5)", SpacingScale.Xs),
        ("Sm (0.75)", SpacingScale.Sm),
        ("Md (1)", SpacingScale.Md),
        ("Lg (1.5)", SpacingScale.Lg),
        ("Xl (2)", SpacingScale.Xl),
        ("Xxl (3)", SpacingScale.Xxl),
        ("Xxxl (4)", SpacingScale.Xxxl),
    };

    private static readonly Rem[] FontSteps = {
        new Rem(0.625f),
        new Rem(0.75f),
        new Rem(0.875f),
        new Rem(1f),
        new Rem(1.25f),
        new Rem(1.5f),
        new Rem(2f),
    };

    [DocVariant("CL_Playground_rem_Conversion")]
    public static DocSample DocsConversion() {
        return new DocSample(() =>
            Box.Create(
                EdgeInsets.All(new Rem(0.5f)),
                BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
                BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderSubtle),
                RadiusSpec.All(RadiusScale.Sm),
                outer => outer.Add(ConversionReadoutNode())
            )
        );
    }

    [DocVariant("CL_Playground_rem_SpacingLadder")]
    public static DocSample DocsSpacingLadder() {
        return new DocSample(() =>
            Box.Create(
                EdgeInsets.All(new Rem(0.5f)),
                BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
                BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderSubtle),
                RadiusSpec.All(RadiusScale.Sm),
                outer => outer.Add(SpacingLadderNode())
            )
        );
    }

    [DocVariant("CL_Playground_rem_FontLadder")]
    public static DocSample DocsFontLadder() {
        return new DocSample(() =>
            Box.Create(
                EdgeInsets.All(new Rem(0.5f)),
                BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
                BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderSubtle),
                RadiusSpec.All(RadiusScale.Sm),
                outer => outer.Add(FontLadderNode())
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() =>
            Box.Create(
                EdgeInsets.All(new Rem(0.5f)),
                BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
                BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderSubtle),
                RadiusSpec.All(RadiusScale.Sm),
                outer => outer.Add(ConversionReadoutNode())
            )
        );
    }

    private static LightweaveNode ConversionReadoutNode() {
        LightweaveNode node = NodeBuilder.New("RemConversion");
        node.PreferredHeight = new Rem(4.5f).ToPixels();
        node.Measure = _ => new Rem(4.5f).ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(theme.GetFont(FontRole.Mono), pixelSize, FontStyle.Normal);
            style.alignment = TextAnchor.UpperLeft;

            string text =
                $"Layout base : {Spacing.BaseUnit}px = 1rem\n" +
                $"Font base   : {Spacing.FontBaseUnit}px = 1rem\n" +
                $"\n" +
                $"0.5rem  -> {0.5f * Spacing.BaseUnit:0.##}px layout / {0.5f * Spacing.FontBaseUnit:0.##}px font\n" +
                $"1rem    -> {Spacing.BaseUnit}px layout / {Spacing.FontBaseUnit}px font\n" +
                $"1.5rem  -> {1.5f * Spacing.BaseUnit:0.##}px layout / {1.5f * Spacing.FontBaseUnit:0.##}px font";

            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
            GUI.Label(RectSnap.Snap(rect), text, style);
            GUI.color = saved;
        };
        return node;
    }

    private static LightweaveNode SpacingLadderNode() {
        LightweaveNode node = NodeBuilder.New("RemSpacingLadder");
        float rowHeight = new Rem(1.5f).ToPixels();
        float totalHeight = rowHeight * LadderSteps.Length;
        node.PreferredHeight = totalHeight;
        node.Measure = _ => totalHeight;
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            int pixelSize = Mathf.RoundToInt(new Rem(0.8125f).ToFontPx());
            GUIStyle labelStyle = GuiStyleCache.GetOrCreate(theme.GetFont(FontRole.Mono), pixelSize, FontStyle.Normal);
            labelStyle.alignment = TextAnchor.MiddleLeft;
            float labelWidth = new Rem(7f).ToPixels();
            float gap = new Rem(0.5f).ToPixels();
            RadiusSpec radius = RadiusSpec.All(RadiusScale.Sm);

            for (int i = 0; i < LadderSteps.Length; i++) {
                (string label, Rem step) = LadderSteps[i];
                Rect row = new Rect(rect.x, rect.y + i * rowHeight, rect.width, rowHeight);
                Rect labelRect = new Rect(row.x, row.y, labelWidth, row.height);
                float barOriginX = labelRect.xMax + gap;
                float barWidth = Mathf.Max(0f, Mathf.Min(step.ToPixels(), row.xMax - barOriginX));
                Rect barRect = new Rect(barOriginX, row.y + rowHeight * 0.25f, barWidth, rowHeight * 0.5f);

                Color saved = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextMuted);
                GUI.Label(RectSnap.Snap(labelRect), label, labelStyle);
                GUI.color = saved;

                if (barWidth > 0f) {
                    PaintBox.Draw(barRect, BackgroundSpec.Of(ThemeSlot.SurfaceAccent), null, radius);
                }
            }
        };
        return node;
    }

    private static LightweaveNode FontLadderNode() {
        LightweaveNode node = NodeBuilder.New("RemFontLadder");
        float totalHeight = 0f;
        float[] rowHeights = new float[FontSteps.Length];
        for (int i = 0; i < FontSteps.Length; i++) {
            rowHeights[i] = Mathf.Max(new Rem(1.25f).ToPixels(), FontSteps[i].ToFontPx() + 4f);
            totalHeight += rowHeights[i];
        }

        node.PreferredHeight = totalHeight;
        node.Measure = _ => totalHeight;
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            float y = rect.y;
            for (int i = 0; i < FontSteps.Length; i++) {
                Rem step = FontSteps[i];
                int pixelSize = Mathf.RoundToInt(step.ToFontPx());
                GUIStyle style = GuiStyleCache.GetOrCreate(theme.GetFont(FontRole.Body), pixelSize, FontStyle.Normal);
                style.alignment = TextAnchor.MiddleLeft;
                Rect row = new Rect(rect.x, y, rect.width, rowHeights[i]);
                Color saved = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
                GUI.Label(RectSnap.Snap(row), $"{step.Value:0.###}rem ({pixelSize}px) The quick brown fox", style);
                GUI.color = saved;
                y += rowHeights[i];
            }
        };
        return node;
    }
}
