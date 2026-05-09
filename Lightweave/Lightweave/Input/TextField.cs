// NOTE: IME and CJK input rely on RimWorld's Verse.Widgets.TextField, which defers to
// Unity's underlying TextField. Chinese/Japanese composition behavior is inherited from
// the engine and has not been verified end-to-end; verification is deferred.

using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Hooks;
using static Cosmere.Lightweave.Hooks.Hooks;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Input;

[Doc(
    Id = "textfield",
    Summary = "Single-line editable text input.",
    WhenToUse = "Capture a short string (name, label, search term).",
    SourcePath = "Lightweave/Lightweave/Input/TextField.cs",
    ShowRtl = true
)]
public static class TextField {
    private const int ShakeFrames = 6;
    private const float ShakeAmplitudePx = 2f;

    public static LightweaveNode Create(
        [DocParam("Current text value.")]
        string value,
        [DocParam("Invoked with the committed text after validation.")]
        Action<string> onChange,
        [DocParam("Optional placeholder rendered when the buffer is empty.")]
        string? placeholder = null,
        [DocParam("Optional predicate that gates commits and triggers a shake on rejection.")]
        Func<string, bool>? validator = null,
        [DocParam("Disables interaction and applies disabled styling.")]
        bool disabled = false,
        [DocParam("Optional shared focus handle for programmatic focus.")]
        UseFocus.FocusHandle? focus = null,
        [DocParam("Optional key disambiguating multiple instances declared on the same line.")]
        object? instanceKey = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        string keySuffix = instanceKey == null ? string.Empty : "#" + instanceKey;
        string focusKey = file + "#tf_focus" + keySuffix;
        string bufferKey = file + "#tf_buffer" + keySuffix;
        string lastGoodKey = file + "#tf_lastGood" + keySuffix;
        string wasFocusedKey = file + "#tf_wasFocused" + keySuffix;
        string shakeKey = file + "#tf_shake" + keySuffix;

        LightweaveNode node = NodeBuilder.New("TextField", line, file);
        node.PreferredHeight = new Rem(1.75f).ToPixels();

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;

            string focusName;
            if (focus != null) {
                focusName = focus.Name;
            }
            else {
                Hooks.Hooks.RefHandle<string> focusNameRef = Hooks.Hooks.UseRef<string>("", line, focusKey);
                if (string.IsNullOrEmpty(focusNameRef.Current)) {
                    focusNameRef.Current = "lw_tf_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                }

                focusName = focusNameRef.Current;
            }

            Hooks.Hooks.StateHandle<string> buffer = Hooks.Hooks.UseState(value ?? string.Empty, line, bufferKey);
            Hooks.Hooks.RefHandle<string> lastGood = Hooks.Hooks.UseRef(value ?? string.Empty, line, lastGoodKey);
            Hooks.Hooks.RefHandle<bool> wasFocused = Hooks.Hooks.UseRef(false, line, wasFocusedKey);
            Hooks.Hooks.StateHandle<int> shakeFrames = Hooks.Hooks.UseState(0, line, shakeKey);

            InteractionState state = InteractionState.Resolve(rect, focusName, disabled);
            InputSurface.Draw(rect, state);

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
                Font tfFont = theme.GetFont(FontRole.Body);
                int tfSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
                Color tfTextColor = theme.GetColor(ThemeSlot.TextPrimary);
                GUIStyle tfStyle = InputSurface.ConfigureChromelessTextFieldStyle(tfFont, tfSize, tfTextColor);
                GUI.SetNextControlName(focusName);
                string next = GUI.TextField(RectSnap.Snap(inner), buffer.Value ?? string.Empty, tfStyle);
                if (!ReferenceEquals(next, buffer.Value) && next != buffer.Value) {
                    buffer.Set(next);
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
                bool accepted = validator == null || validator(candidate);
                if (accepted) {
                    lastGood.Current = candidate;
                    onChange?.Invoke(candidate);
                }
                else {
                    buffer.Set(lastGood.Current ?? string.Empty);
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

    [DocVariant("CL_Playground_Label_Filled")]
    public static DocSample DocsFilled() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<string> s = UseState("Stormlight");
        return new DocSample(() => Create(
            s.Value,
            v => s.Set(v),
            (string)"CL_Playground_Controls_TextField_Placeholder".Translate(),
            disabled: forced
        ));
    }

    [DocVariant("CL_Playground_Label_Empty")]
    public static DocSample DocsEmpty() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<string> s = UseState(string.Empty);
        return new DocSample(() => Create(
            s.Value,
            v => s.Set(v),
            (string)"CL_Playground_Controls_TextField_Placeholder".Translate(),
            disabled: forced
        ));
    }

    [DocState("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<string> s = UseState("Default");
        return new DocSample(() => Create(s.Value, v => s.Set(v), disabled: forced));
    }

    [DocState("CL_Playground_Label_Hover")]
    public static DocSample DocsHover() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<string> s = UseState("Hover");
        return new DocSample(() => Create(s.Value, v => s.Set(v), disabled: forced));
    }

    [DocState("CL_Playground_Label_Disabled")]
    public static DocSample DocsDisabled() {
        StateHandle<string> s = UseState("Disabled");
        return new DocSample(() => Create(s.Value, v => s.Set(v), disabled: true));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        StateHandle<string> s = UseState("Stormlight");
        return new DocSample(() => Create(s.Value, v => s.Set(v)));
    }
}