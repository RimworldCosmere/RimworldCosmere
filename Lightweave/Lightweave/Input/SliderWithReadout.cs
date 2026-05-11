using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.Input;

[Doc(
    Id = "slider-readout",
    Summary = "Slider paired with a right-aligned mono-font numeric readout.",
    WhenToUse = "Settings rows where the user needs to see the exact current value (volume, scale, framerate cap). Composes the existing Slider primitive; the readout is a separate column so the slider track gets the full available width.",
    SourcePath = "Lightweave/Lightweave/Input/SliderWithReadout.cs"
)]
public static class SliderWithReadout {
    public static LightweaveNode Create(
        [DocParam("Current value.")]
        float value,
        [DocParam("Invoked when the user releases the drag with the new value.")]
        Action<float> onChange,
        [DocParam("Minimum value.")]
        float min = 0f,
        [DocParam("Maximum value.")]
        float max = 1f,
        [DocParam("Snap step. 0 = continuous.")]
        float step = 0f,
        [DocParam("Optional format function for the readout. Defaults to two-decimal invariant culture.")]
        Func<float, string>? format = null,
        [DocParam("Readout column width. Defaults to 4rem.")]
        Rem? readoutWidth = null,
        [DocParam("Disable interaction. Slider is greyed out and does not respond.")]
        bool disabled = false,
        [DocParam("When true, onChange fires every frame during drag instead of only on release.")]
        bool live = false,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Rem rw = readoutWidth ?? new Rem(4f);
        Func<float, string> fmt = format ?? (v => v.ToString("0.00", CultureInfo.InvariantCulture));
        string[]? mergedClasses = StyleExtensions.PrependClass("slider-with-readout", classes);

        return HStack.Create(
            gap: SpacingScale.Md,
            children: h => {
                h.AddFlex(Slider.Create(
                    value: value,
                    onChange: onChange,
                    min: min,
                    max: max,
                    step: step,
                    format: fmt,
                    disabled: disabled,
                    live: live,
                    showReadout: false,
                    line: line,
                    file: file
                ));
                h.Add(BuildReadout(value, fmt, disabled, line, file), rw.ToPixels());
            },
            style: style,
            classes: mergedClasses,
            id: id,
            line: line,
            file: file
        );
    }

    private static LightweaveNode BuildReadout(float value, Func<float, string> fmt, bool disabled, int line, string file) {
        LightweaveNode node = NodeBuilder.New("SliderReadout", line, file);
        node.PreferredHeight = new Rem(1.25f).ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = theme.GetFont(FontRole.Mono);
            int px = Mathf.RoundToInt(new Rem(0.78f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(font, px, FontStyle.Normal);
            style.alignment = TextAnchor.MiddleRight;
            style.clipping = TextClipping.Clip;

            string text = fmt(value);
            Color saved = GUI.color;
            GUI.color = disabled
                ? theme.GetColor(ThemeSlot.TextMuted)
                : theme.GetColor(ThemeSlot.TextPrimary);
            GUI.Label(RectSnap.Snap(rect), text, style);
            GUI.color = saved;
        };
        return node;
    }

    [DocVariant("CL_Playground_slider-readout_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => {
            float v = 0.5f;
            return SliderWithReadout.Create(v, _ => { });
        });
    }

    [DocVariant("CL_Playground_slider-readout_Percent", Order = 1)]
    public static DocSample DocsPercent() {
        return new DocSample(() => {
            float v = 0.75f;
            return SliderWithReadout.Create(
                value: v,
                onChange: _ => { },
                format: x => Mathf.RoundToInt(x * 100f) + "%"
            );
        });
    }

    [DocVariant("CL_Playground_slider-readout_Disabled", Order = 2)]
    public static DocSample DocsDisabled() {
        return new DocSample(() => SliderWithReadout.Create(0.3f, _ => { }, disabled: true));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => SliderWithReadout.Create(0.6f, _ => { }, format: v => Mathf.RoundToInt(v * 100f) + "%"));
    }
}
