using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Input;

public static class NumberField {
    private const int ShakeFrames = 6;
    private const float ShakeAmplitudePx = 2f;

    public static LightweaveNode Create(
        float value,
        Action<float> onChange,
        float min = float.MinValue,
        float max = float.MaxValue,
        Func<string, float?>? parse = null,
        Func<float, string>? format = null,
        string? placeholder = null,
        bool disabled = false,
        bool allowDecimal = true,
        int decimalPlaces = 2,
        object? instanceKey = null,
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0
    ) {
        string callerFile = caller ?? string.Empty;
        int callerLine = line;
        string keySuffix = instanceKey == null ? string.Empty : "#" + instanceKey;
        string focusKey = callerFile + "#nf_focus" + keySuffix;
        string bufferKey = callerFile + "#nf_buffer" + keySuffix;
        string lastGoodKey = callerFile + "#nf_lastGood" + keySuffix;
        string syncedValueKey = callerFile + "#nf_syncedValue" + keySuffix;
        string syncedFromKey = callerFile + "#nf_syncedFrom" + keySuffix;
        string wasFocusedKey = callerFile + "#nf_wasFocused" + keySuffix;
        string shakeKey = callerFile + "#nf_shake" + keySuffix;

        LightweaveNode node = NodeBuilder.New("NumberField", callerLine, callerFile);
        node.PreferredHeight = new Rem(1.75f).ToPixels();

        bool localAllowDecimal = allowDecimal;
        int localDecimalPlaces = Mathf.Max(0, decimalPlaces);
        Func<string, float?> effectiveParse = parse ?? (text => DefaultParse(text, localAllowDecimal));
        Func<float, string> effectiveFormat = format ?? (v => DefaultFormat(v, localAllowDecimal, localDecimalPlaces));

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;

            Hooks.Hooks.RefHandle<string> focusNameRef = Hooks.Hooks.UseRef<string>("", callerLine, focusKey);
            if (string.IsNullOrEmpty(focusNameRef.Current)) {
                focusNameRef.Current = "lw_nf_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            string focusName = focusNameRef.Current;

            float clampedInitial = Mathf.Clamp(value, min, max);
            Hooks.Hooks.StateHandle<string> buffer = Hooks.Hooks.UseState(
                effectiveFormat(clampedInitial),
                callerLine,
                bufferKey
            );
            Hooks.Hooks.RefHandle<float> lastGood = Hooks.Hooks.UseRef(clampedInitial, callerLine, lastGoodKey);
            Hooks.Hooks.RefHandle<bool> syncedValue = Hooks.Hooks.UseRef(false, callerLine, syncedValueKey);
            Hooks.Hooks.RefHandle<float> syncedFrom = Hooks.Hooks.UseRef(clampedInitial, callerLine, syncedFromKey);
            Hooks.Hooks.RefHandle<bool> wasFocused = Hooks.Hooks.UseRef(false, callerLine, wasFocusedKey);
            Hooks.Hooks.StateHandle<int> shakeFrames = Hooks.Hooks.UseState(0, callerLine, shakeKey);

            bool isFocusedThisFrame = GUI.GetNameOfFocusedControl() == focusName;
            if (!isFocusedThisFrame &&
                (!syncedValue.Current || !Mathf.Approximately(syncedFrom.Current, clampedInitial))) {
                buffer.Set(effectiveFormat(clampedInitial));
                lastGood.Current = clampedInitial;
                syncedFrom.Current = clampedInitial;
                syncedValue.Current = true;
            }

            InteractionState state = InteractionState.Resolve(rect, focusName, disabled);
            InputSurface.Draw(rect, theme, state);

            float padX = InputSurface.PaddingX.ToPixels();
            float padY = InputSurface.PaddingY.ToPixels();
            Rect inner = new Rect(rect.x + padX, rect.y + padY, rect.width - padX * 2f, rect.height - padY * 2f);

            if (shakeFrames.Value > 0) {
                float sign = shakeFrames.Value % 2 == 0 ? 1f : -1f;
                inner = new Rect(inner.x + sign * ShakeAmplitudePx, inner.y, inner.width, inner.height);
                shakeFrames.Set(shakeFrames.Value - 1);
            }

            bool showPlaceholder =
                !state.Focused && string.IsNullOrEmpty(buffer.Value) && !string.IsNullOrEmpty(placeholder);

            if (showPlaceholder) {
                Font phFont = theme.GetFont(FontRole.Body);
                int phSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
                GUIStyle phStyle = GuiStyleCache.Get(phFont, phSize);
                phStyle.alignment = TextAnchor.MiddleLeft;
                Color savedColor = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextMuted);
                GUI.Label(RectSnap.Snap(inner), placeholder, phStyle);
                GUI.color = savedColor;
            }

            Event e = Event.current;
            bool enterPressed = e.type == EventType.KeyDown &&
                                (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) &&
                                state.Focused;

            if (disabled) {
                Font roFont = theme.GetFont(FontRole.Body);
                int roSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
                GUIStyle roStyle = GuiStyleCache.Get(roFont, roSize);
                roStyle.alignment = TextAnchor.MiddleLeft;
                Color savedColor = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextMuted);
                GUI.Label(RectSnap.Snap(inner), buffer.Value ?? string.Empty, roStyle);
                GUI.color = savedColor;
            } else {
                Font nfFont = theme.GetFont(FontRole.Body);
                int nfSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
                Color nfTextColor = theme.GetColor(ThemeSlot.TextPrimary);
                GUIStyle nfStyle = InputSurface.GetChromelessTextFieldStyle(nfFont, nfSize, nfTextColor);
                GUI.SetNextControlName(focusName);
                string next = GUI.TextField(RectSnap.Snap(inner), buffer.Value ?? string.Empty, nfStyle);
                string sanitized = SanitizeNumeric(next, localAllowDecimal);
                if (sanitized != buffer.Value) {
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
                    syncedFrom.Current = clamped;
                    syncedValue.Current = true;
                    buffer.Set(effectiveFormat(clamped));
                    onChange?.Invoke(clamped);
                } else {
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
}