using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Input;

public static class NumberField
{
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
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0)
    {
        LightweaveNode node = NodeBuilder.New("NumberField", line, caller ?? string.Empty);

        Func<string, float?> effectiveParse = parse ?? DefaultParse;
        Func<float, string> effectiveFormat = format ?? DefaultFormat;

        node.Paint = (rect, paintChildren) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;

            Hooks.Hooks.RefHandle<string> focusNameRef = Hooks.Hooks.UseRef<string>("");
            if (string.IsNullOrEmpty(focusNameRef.Current))
            {
                focusNameRef.Current = "lw_nf_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            }
            string focusName = focusNameRef.Current;

            float clampedInitial = Mathf.Clamp(value, min, max);
            Hooks.Hooks.StateHandle<string> buffer = Hooks.Hooks.UseState<string>(effectiveFormat(clampedInitial));
            Hooks.Hooks.RefHandle<float> lastGood = Hooks.Hooks.UseRef<float>(clampedInitial);
            Hooks.Hooks.RefHandle<bool> syncedValue = Hooks.Hooks.UseRef<bool>(false);
            Hooks.Hooks.RefHandle<float> syncedFrom = Hooks.Hooks.UseRef<float>(clampedInitial);
            Hooks.Hooks.RefHandle<bool> wasFocused = Hooks.Hooks.UseRef<bool>(false);
            Hooks.Hooks.StateHandle<int> shakeFrames = Hooks.Hooks.UseState<int>(0);

            bool isFocusedThisFrame = GUI.GetNameOfFocusedControl() == focusName;
            if (!isFocusedThisFrame && (!syncedValue.Current || !Mathf.Approximately(syncedFrom.Current, clampedInitial)))
            {
                buffer.Set(effectiveFormat(clampedInitial));
                lastGood.Current = clampedInitial;
                syncedFrom.Current = clampedInitial;
                syncedValue.Current = true;
            }

            InteractionState state = InteractionState.Resolve(rect, focusName, disabled);
            InputSurface.Draw(rect, theme, state);

            float padX = new Rem(0.5f).ToPixels();
            Rect inner = new Rect(rect.x + padX, rect.y, rect.width - padX * 2f, rect.height);

            if (shakeFrames.Value > 0)
            {
                float sign = (shakeFrames.Value % 2 == 0) ? 1f : -1f;
                inner = new Rect(inner.x + sign * ShakeAmplitudePx, inner.y, inner.width, inner.height);
                shakeFrames.Set(shakeFrames.Value - 1);
            }

            bool showPlaceholder = !state.Focused
                && string.IsNullOrEmpty(buffer.Value)
                && !string.IsNullOrEmpty(placeholder);

            if (showPlaceholder)
            {
                Font phFont = theme.GetFont(FontRole.Body);
                int phSize = Mathf.RoundToInt(new Rem(1f).ToPixels());
                GUIStyle phStyle = GuiStyleCache.Get(phFont, phSize, FontStyle.Normal);
                phStyle.alignment = TextAnchor.MiddleLeft;
                Color savedColor = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextMuted);
                GUI.Label(RectSnap.Snap(inner), placeholder, phStyle);
                GUI.color = savedColor;
            }

            Event e = Event.current;
            bool enterPressed = e.type == EventType.KeyDown
                && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                && state.Focused;

            if (disabled)
            {
                Font roFont = theme.GetFont(FontRole.Body);
                int roSize = Mathf.RoundToInt(new Rem(1f).ToPixels());
                GUIStyle roStyle = GuiStyleCache.Get(roFont, roSize, FontStyle.Normal);
                roStyle.alignment = TextAnchor.MiddleLeft;
                Color savedColor = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextMuted);
                GUI.Label(RectSnap.Snap(inner), buffer.Value ?? string.Empty, roStyle);
                GUI.color = savedColor;
            }
            else
            {
                GUI.SetNextControlName(focusName);
                string next = Verse.Widgets.TextField(inner, buffer.Value ?? string.Empty);
                if (next != buffer.Value)
                {
                    buffer.Set(next);
                }
            }

            bool isFocusedNow = GUI.GetNameOfFocusedControl() == focusName;
            bool focusLost = wasFocused.Current && !isFocusedNow;
            wasFocused.Current = isFocusedNow;

            if (enterPressed || focusLost)
            {
                string candidate = buffer.Value ?? string.Empty;
                float? parsed = effectiveParse(candidate);
                if (parsed.HasValue)
                {
                    float clamped = Mathf.Clamp(parsed.Value, min, max);
                    lastGood.Current = clamped;
                    syncedFrom.Current = clamped;
                    syncedValue.Current = true;
                    buffer.Set(effectiveFormat(clamped));
                    onChange?.Invoke(clamped);
                }
                else
                {
                    buffer.Set(effectiveFormat(lastGood.Current));
                    shakeFrames.Set(ShakeFrames);
                }
                if (enterPressed)
                {
                    e.Use();
                }
            }

            paintChildren();
        };

        return node;
    }

    private static float? DefaultParse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }
        if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            return result;
        }
        return null;
    }

    private static string DefaultFormat(float value)
    {
        return value.ToString("F2", CultureInfo.InvariantCulture);
    }
}
