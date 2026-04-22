// NOTE: IME and CJK input rely on RimWorld's Verse.Widgets.TextField, which defers to
// Unity's underlying TextField. Chinese/Japanese composition behavior is inherited from
// the engine and has not been verified end-to-end; verification is deferred.
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Input;

public static class TextField
{
    private const int ShakeFrames = 6;
    private const float ShakeAmplitudePx = 2f;

    public static LightweaveNode Create(
        string value,
        Action<string> onChange,
        string? placeholder = null,
        Func<string, bool>? validator = null,
        bool disabled = false,
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0)
    {
        LightweaveNode node = NodeBuilder.New("TextField", line, caller ?? string.Empty);

        node.Paint = (rect, paintChildren) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;

            Hooks.Hooks.RefHandle<string> focusNameRef = Hooks.Hooks.UseRef<string>("");
            if (string.IsNullOrEmpty(focusNameRef.Current))
            {
                focusNameRef.Current = "lw_tf_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            }
            string focusName = focusNameRef.Current;

            Hooks.Hooks.StateHandle<string> buffer = Hooks.Hooks.UseState<string>(value ?? string.Empty);
            Hooks.Hooks.RefHandle<string> lastGood = Hooks.Hooks.UseRef<string>(value ?? string.Empty);
            Hooks.Hooks.RefHandle<bool> wasFocused = Hooks.Hooks.UseRef<bool>(false);
            Hooks.Hooks.StateHandle<int> shakeFrames = Hooks.Hooks.UseState<int>(0);

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
                if (!ReferenceEquals(next, buffer.Value) && next != buffer.Value)
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
                bool accepted = validator == null || validator(candidate);
                if (accepted)
                {
                    lastGood.Current = candidate;
                    onChange?.Invoke(candidate);
                }
                else
                {
                    buffer.Set(lastGood.Current ?? string.Empty);
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
}
