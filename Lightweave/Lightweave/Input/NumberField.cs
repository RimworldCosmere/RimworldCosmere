using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Hooks.Hooks;

namespace Cosmere.Lightweave.Input;

[Doc(
    Id = "numberfield",
    Summary = "Numeric input with bounds, parsing, and formatting.",
    WhenToUse = "Capture a single numeric value with optional min/max clamping.",
    SourcePath = "Lightweave/Lightweave/Input/NumberField.cs",
    ShowRtl = true
)]
public static class NumberField {
    private const int ShakeFrames = 6;
    private const float ShakeAmplitudePx = 2f;

    public static LightweaveNode Create(
        [DocParam("Current numeric value.")]
        float value,
        [DocParam("Invoked with the committed value after parsing and clamping.")]
        Action<float> onChange,
        [DocParam("Inclusive minimum bound.")]
        float min = float.MinValue,
        [DocParam("Inclusive maximum bound.")]
        float max = float.MaxValue,
        [DocParam("Optional parser overriding the default numeric parsing.")]
        Func<string, float?>? parse = null,
        [DocParam("Optional formatter overriding the default decimal rendering.")]
        Func<float, string>? format = null,
        [DocParam("Optional placeholder rendered when the buffer is empty.")]
        string? placeholder = null,
        [DocParam("Disables interaction and applies disabled styling.")]
        bool disabled = false,
        [DocParam("When false, only integer characters are accepted.")]
        bool allowDecimal = true,
        [DocParam("Maximum decimal places used by the default formatter.")]
        int decimalPlaces = 2,
        [DocParam("Optional key disambiguating multiple instances declared on the same line.")]
        object? instanceKey = null,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        string keySuffix = instanceKey == null ? string.Empty : "#" + instanceKey;
        string focusKey = file + "#nf_focus" + keySuffix;
        string bufferKey = file + "#nf_buffer" + keySuffix;
        string lastGoodKey = file + "#nf_lastGood" + keySuffix;
        string lastSeenKey = file + "#nf_lastSeen" + keySuffix;
        string wasFocusedKey = file + "#nf_wasFocused" + keySuffix;
        string shakeKey = file + "#nf_shake" + keySuffix;

        LightweaveNode node = NodeBuilder.New("NumberField", line, file);
        node.ApplyStyling("number-field", style, classes, id);
        node.PreferredHeight = new Rem(1.75f).ToPixels();

        bool localAllowDecimal = allowDecimal;
        int localDecimalPlaces = Mathf.Max(0, decimalPlaces);
        Func<string, float?> effectiveParse = parse ?? (text => DefaultParse(text, localAllowDecimal));
        Func<float, string> effectiveFormat = format ?? (v => DefaultFormat(v, localAllowDecimal, localDecimalPlaces));

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;

            Hooks.Hooks.RefHandle<string> focusNameRef = Hooks.Hooks.UseRef<string>("", line, focusKey);
            if (string.IsNullOrEmpty(focusNameRef.Current)) {
                focusNameRef.Current = "lw_nf_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            string focusName = focusNameRef.Current;

            float clampedInitial = Mathf.Clamp(value, min, max);
            Hooks.Hooks.StateHandle<string> buffer = Hooks.Hooks.UseState(
                effectiveFormat(clampedInitial),
                line,
                bufferKey
            );
            Hooks.Hooks.RefHandle<float> lastGood = Hooks.Hooks.UseRef(clampedInitial, line, lastGoodKey);
            Hooks.Hooks.RefHandle<float> lastSeen = Hooks.Hooks.UseRef(clampedInitial, line, lastSeenKey);
            Hooks.Hooks.RefHandle<bool> wasFocused = Hooks.Hooks.UseRef(false, line, wasFocusedKey);
            Hooks.Hooks.StateHandle<int> shakeFrames = Hooks.Hooks.UseState(0, line, shakeKey);

            bool isFocusedThisFrame = GUI.GetNameOfFocusedControl() == focusName;
            if (!isFocusedThisFrame && !Mathf.Approximately(lastSeen.Current, clampedInitial)) {
                buffer.Set(effectiveFormat(clampedInitial));
                lastGood.Current = clampedInitial;
                lastSeen.Current = clampedInitial;
            }

            InteractionState state = InteractionState.Resolve(rect, focusName, disabled);
            InputSurface.Draw(rect, state);

            float padX = InputSurface.PaddingX.ToPixels();
            float padY = InputSurface.PaddingY.ToPixels();
            Rect inner = new Rect(rect.x + padX, rect.y + padY, rect.width - padX * 2f, rect.height - padY * 2f);

            if (shakeFrames.Value > 0 && Event.current.type == EventType.Repaint) {
                float sign = shakeFrames.Value % 2 == 0 ? 1f : -1f;
                inner = new Rect(inner.x + sign * ShakeAmplitudePx, inner.y, inner.width, inner.height);
                shakeFrames.Set(shakeFrames.Value - 1);
            }

            bool showPlaceholder =
                !state.Focused && string.IsNullOrEmpty(buffer.Value) && !string.IsNullOrEmpty(placeholder);

            if (showPlaceholder) {
                InputSurface.DrawPlaceholder(inner, placeholder, theme);
            }

            Event e = Event.current;
            bool enterPressed = e.type == EventType.KeyDown &&
                                (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) &&
                                state.Focused;

            if (disabled) {
                InputSurface.DrawReadOnlyValue(inner, buffer.Value ?? string.Empty, theme);
            }
            else {
                Font nfFont = theme.GetFont(FontRole.Body);
                int nfSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
                Color nfTextColor = theme.GetColor(ThemeSlot.TextPrimary);
                GUIStyle nfStyle = InputSurface.ConfigureChromelessTextFieldStyle(nfFont, nfSize, nfTextColor);
                GUI.SetNextControlName(focusName);
                string next = GUI.TextField(RectSnap.Snap(inner), buffer.Value ?? string.Empty, nfStyle);
                if (!ReferenceEquals(next, buffer.Value) && next != buffer.Value) {
                    string sanitized = SanitizeNumeric(next, localAllowDecimal);
                    buffer.Set(sanitized);
                }
            }

            if (!disabled && e.type == EventType.MouseDown && e.button == 0 && !rect.Contains(e.mousePosition)) {
                if (GUI.GetNameOfFocusedControl() == focusName) {
                    GUI.FocusControl(null);
                }
            }

            bool isFocusedNow = GUI.GetNameOfFocusedControl() == focusName;
            bool focusLost = wasFocused.Current && !isFocusedNow;
            wasFocused.Current = isFocusedNow;

            if (enterPressed || focusLost) {
                string candidate = buffer.Value ?? string.Empty;
                float? parsed = effectiveParse(candidate);
                if (parsed.HasValue) {
                    float clamped = Mathf.Clamp(parsed.Value, min, max);
                    lastGood.Current = clamped;
                    lastSeen.Current = clamped;
                    buffer.Set(effectiveFormat(clamped));
                    onChange?.Invoke(clamped);
                }
                else {
                    buffer.Set(effectiveFormat(lastGood.Current));
                    shakeFrames.Set(ShakeFrames);
                }

                if (enterPressed) {
                    e.Use();
                }
            }

            paintChildren();
        };

        return node;
    }

    private static float? DefaultParse(string text, bool allowDecimal) {
        if (string.IsNullOrWhiteSpace(text)) {
            return null;
        }

        NumberStyles styles = allowDecimal ? NumberStyles.Float : NumberStyles.Integer;
        if (float.TryParse(text, styles, CultureInfo.InvariantCulture, out float result)) {
            return allowDecimal ? result : Mathf.Round(result);
        }

        return null;
    }

    private static string DefaultFormat(float value, bool allowDecimal, int decimalPlaces) {
        if (!allowDecimal) {
            return Mathf.RoundToInt(value).ToString(CultureInfo.InvariantCulture);
        }

        string spec = "F" + decimalPlaces.ToString(CultureInfo.InvariantCulture);
        return value.ToString(spec, CultureInfo.InvariantCulture);
    }

    private static string SanitizeNumeric(string text, bool allowDecimal) {
        if (string.IsNullOrEmpty(text)) {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder(text.Length);
        bool seenDot = false;
        for (int i = 0; i < text.Length; i++) {
            char c = text[i];
            if (char.IsDigit(c)) {
                sb.Append(c);
                continue;
            }

            if (c == '-' && sb.Length == 0) {
                sb.Append(c);
                continue;
            }

            if (allowDecimal && c == '.' && !seenDot) {
                sb.Append(c);
                seenDot = true;
            }
        }

        return sb.ToString();
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<float> s = UseState(42f);
        return new DocSample(() => Create(
            s.Value,
            v => s.Set(v),
            0f,
            100f,
            placeholder: (string)"CL_Playground_Controls_NumberField_Label".Translate(),
            disabled: forced
        ));
    }

    [DocState("CL_Playground_Label_Default")]
    public static DocSample DocsDefaultState() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<float> s = UseState(42f);
        return new DocSample(() => Create(s.Value, v => s.Set(v), 0f, 100f, disabled: forced));
    }

    [DocState("CL_Playground_Label_Hover")]
    public static DocSample DocsHover() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<float> s = UseState(7f);
        return new DocSample(() => Create(s.Value, v => s.Set(v), 0f, 100f, disabled: forced));
    }

    [DocState("CL_Playground_Label_Disabled")]
    public static DocSample DocsDisabled() {
        StateHandle<float> s = UseState(13f);
        return new DocSample(() => Create(s.Value, v => s.Set(v), 0f, 100f, disabled: true));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        StateHandle<float> s = UseState(42f);
        return new DocSample(() => Create(s.Value, v => s.Set(v), 0f, 100f));
    }
}