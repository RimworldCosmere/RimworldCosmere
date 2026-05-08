using System;
using System.Runtime.CompilerServices;
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
    Id = "textarea",
    Summary = "Multi-line editable text input that grows with content.",
    WhenToUse = "Capture longer prose: notes, descriptions, multi-line input.",
    SourcePath = "Lightweave/Lightweave/Input/TextArea.cs"
)]
public static class TextArea {
    public static LightweaveNode Create(
        [DocParam("Current text value.")]
        string value,
        [DocParam("Invoked with the committed text after focus is lost.")]
        Action<string> onChange,
        [DocParam("Optional placeholder rendered when the buffer is empty.")]
        string? placeholder = null,
        [DocParam("Minimum number of visible rows before content grows.")]
        int minRows = 3,
        [DocParam("Maximum number of visible rows before scrolling.")]
        int maxRows = 8,
        [DocParam("Renders the value without an editable surface.")]
        bool readOnly = false,
        [DocParam("Disables interaction and applies disabled styling.")]
        bool disabled = false,
        [DocParam("Optional key disambiguating multiple instances declared on the same line.")]
        object? instanceKey = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        string keySuffix = instanceKey == null ? string.Empty : "#" + instanceKey;
        string focusKey = file + "#ta_focus" + keySuffix;
        string bufferKey = file + "#ta_buffer" + keySuffix;
        string wasFocusedKey = file + "#ta_wasFocused" + keySuffix;

        LightweaveNode node = NodeBuilder.New("TextArea", line, file);
        float lineHeightPx = new Rem(1.5f).ToPixels();
        int initialRows = Mathf.Clamp(
            CountRows(value ?? string.Empty),
            Mathf.Max(1, minRows),
            Mathf.Max(minRows, maxRows)
        );
        node.PreferredHeight = initialRows * lineHeightPx;

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;

            Hooks.Hooks.RefHandle<string> focusNameRef = Hooks.Hooks.UseRef<string>("", line, focusKey);
            if (string.IsNullOrEmpty(focusNameRef.Current)) {
                focusNameRef.Current = "lw_ta_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            string focusName = focusNameRef.Current;

            Hooks.Hooks.StateHandle<string> buffer = Hooks.Hooks.UseState(value ?? string.Empty, line, bufferKey);
            Hooks.Hooks.RefHandle<bool> wasFocused = Hooks.Hooks.UseRef(false, line, wasFocusedKey);

            float lineHeight = new Rem(1.5f).ToPixels();
            int contentRows = CountRows(buffer.Value ?? string.Empty);
            int clampedRows = Mathf.Clamp(contentRows, Mathf.Max(1, minRows), Mathf.Max(minRows, maxRows));
            float resolvedHeight = clampedRows * lineHeight;
            Rect surfaceRect = new Rect(rect.x, rect.y, rect.width, resolvedHeight);

            InteractionState state = InteractionState.Resolve(surfaceRect, focusName, disabled);
            InputSurface.Draw(surfaceRect, state);

            float padX = InputSurface.PaddingX.ToPixels();
            float padY = InputSurface.PaddingY.ToPixels();
            Rect inner = new Rect(
                surfaceRect.x + padX,
                surfaceRect.y + padY,
                surfaceRect.width - padX * 2f,
                surfaceRect.height - padY * 2f
            );

            bool showPlaceholder =
                !state.Focused && string.IsNullOrEmpty(buffer.Value) && !string.IsNullOrEmpty(placeholder);

            if (showPlaceholder) {
                InputSurface.DrawPlaceholder(inner, placeholder, theme, TextAnchor.UpperLeft);
            }

            if (disabled) {
                InputSurface.DrawReadOnlyValue(inner, buffer.Value ?? string.Empty, theme, TextAnchor.UpperLeft);
            }
            else {
                Font taFont = theme.GetFont(FontRole.Body);
                int taSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
                Color taTextColor = theme.GetColor(ThemeSlot.TextPrimary);
                GUIStyle taStyle = InputSurface.ConfigureChromelessTextAreaStyle(taFont, taSize, taTextColor);
                GUI.SetNextControlName(focusName);
                string next = GUI.TextArea(RectSnap.Snap(inner), buffer.Value ?? string.Empty, taStyle);
                if (next != buffer.Value) {
                    buffer.Set(next);
                }
            }

            Event evt = Event.current;
            if (!disabled &&
                evt.type == EventType.MouseDown &&
                evt.button == 0 &&
                !surfaceRect.Contains(evt.mousePosition)) {
                if (GUI.GetNameOfFocusedControl() == focusName) {
                    GUI.FocusControl(null);
                }
            }

            bool isFocusedNow = GUI.GetNameOfFocusedControl() == focusName;
            bool focusLost = wasFocused.Current && !isFocusedNow;
            wasFocused.Current = isFocusedNow;

            if (focusLost && !readOnly) {
                onChange?.Invoke(buffer.Value ?? string.Empty);
            }

            paintChildren();
        };

        return node;
    }

    private static int CountRows(string text) {
        if (string.IsNullOrEmpty(text)) {
            return 1;
        }

        int rows = 1;
        for (int i = 0; i < text.Length; i++) {
            if (text[i] == '\n') {
                rows++;
            }
        }

        return rows;
    }

    [DocVariant("CC_Playground_Label_Filled")]
    public static DocSample DocsFilled() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<string> s = UseState("Multi-line sample.");
        return new DocSample(() => Create(
            s.Value,
            v => s.Set(v),
            (string)"CC_Playground_Controls_TextArea_Placeholder".Translate(),
            2,
            3,
            disabled: forced
        ));
    }

    [DocVariant("CC_Playground_Label_Empty")]
    public static DocSample DocsEmpty() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<string> s = UseState(string.Empty);
        return new DocSample(() => Create(
            s.Value,
            v => s.Set(v),
            (string)"CC_Playground_Controls_TextArea_Placeholder".Translate(),
            2,
            3,
            disabled: forced
        ));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        StateHandle<string> s = UseState("Notes about the bond.");
        return new DocSample(() => Create(s.Value, v => s.Set(v)));
    }
}