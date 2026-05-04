using System;
using System.Runtime.CompilerServices;
using System.Text;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Hooks;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Input;

public readonly record struct KeyBinding(KeyCode Key, KeyModifiers Modifiers);

[Doc(
    Id = "keybinding",
    Summary = "Captures a single keyboard shortcut with optional modifiers.",
    WhenToUse = "Bind a hotkey: click to record, then press the desired combination.",
    SourcePath = "Lightweave/Lightweave/Input/KeyBindingField.cs"
)]
public static class KeyBindingField {
    private const string ClearGlyph = "×";

    public static LightweaveNode Create(
        [DocParam("Currently bound key combination.")]
        KeyBinding value,
        [DocParam("Invoked with the new binding when a combination is recorded.")]
        Action<KeyBinding> onChange,
        [DocParam("Disables interaction and applies disabled styling.")]
        bool disabled = false,
        [DocParam("Optional key disambiguating multiple instances declared on the same line.")]
        object? instanceKey = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        string keySuffix = instanceKey == null ? string.Empty : "#" + instanceKey;
        string recordingKey = file + "#kbf_recording" + keySuffix;

        LightweaveNode node = NodeBuilder.New("KeyBindingField", line, file);
        node.PreferredHeight = new Rem(1.75f).ToPixels();

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;

            Hooks.Hooks.StateHandle<bool> recording = Hooks.Hooks.UseState(false, line, recordingKey);

            bool isRecording = recording.Value && !disabled;
            bool mouseOverField = Mouse.IsOver(rect);
            if (disabled && mouseOverField) {
                CursorOverrides.MarkDisabledHover();
            }

            InteractionState state = new InteractionState(
                !disabled && mouseOverField,
                !disabled && mouseOverField && UnityEngine.Input.GetMouseButton(0),
                isRecording,
                disabled
            );

            InputSurface.Draw(rect, state);

            float padX = SpacingScale.Sm.ToPixels();
            float glyphSize = new Rem(1f).ToPixels();
            bool hasBinding = value.Key != KeyCode.None;
            bool showClear = hasBinding && !disabled && !isRecording;

            bool clearOnRight = dir == Direction.Ltr;
            Rect clearRect = clearOnRight
                ? new Rect(rect.xMax - padX - glyphSize, rect.y, glyphSize, rect.height)
                : new Rect(rect.x + padX, rect.y, glyphSize, rect.height);

            float leftLabelX = clearOnRight ? rect.x + padX :
                showClear ? clearRect.xMax + SpacingScale.Xs.ToPixels() : rect.x + padX;
            float rightLabelX = clearOnRight
                ? showClear ? clearRect.x - SpacingScale.Xs.ToPixels() : rect.xMax - padX
                : rect.xMax - padX;

            Rect labelRect = new Rect(
                leftLabelX,
                rect.y,
                Mathf.Max(0f, rightLabelX - leftLabelX),
                rect.height
            );

            DrawLabel(labelRect, theme, value, isRecording, disabled);

            if (showClear) {
                DrawClearButton(clearRect, theme, onChange);
            }

            Event e = Event.current;

            if (!disabled &&
                !isRecording &&
                e.type == EventType.MouseDown &&
                e.button == 0 &&
                rect.Contains(e.mousePosition) &&
                !(showClear && clearRect.Contains(e.mousePosition))) {
                recording.Set(true);
                GUI.FocusControl(null);
                GUIUtility.keyboardControl = 0;
                e.Use();
            }

            if (isRecording && e.type == EventType.MouseDown && !rect.Contains(e.mousePosition)) {
                recording.Set(false);
            }

            bool isKeyDown = e.type == EventType.KeyDown && e.keyCode != KeyCode.None;
            if (isRecording && isKeyDown) {
                if (e.keyCode == KeyCode.Escape && !e.control && !e.shift && !e.alt) {
                    recording.Set(false);
                    e.Use();
                }
                else if (!IsModifierOnly(e.keyCode)) {
                    KeyModifiers mods = KeyModifiers.None;
                    if (e.control || e.command) {
                        mods |= KeyModifiers.Control;
                    }

                    if (e.shift) {
                        mods |= KeyModifiers.Shift;
                    }

                    if (e.alt) {
                        mods |= KeyModifiers.Alt;
                    }

                    onChange?.Invoke(new KeyBinding(e.keyCode, mods));
                    recording.Set(false);
                    e.Use();
                }
            }

            paintChildren();
        };

        return node;
    }

    private static void DrawLabel(Rect rect, Theme.Theme theme, KeyBinding value, bool recording, bool disabled) {
        Font font = theme.GetFont(FontRole.Body);
        int pixelSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
        FontStyle fontStyle = recording ? FontStyle.Italic : FontStyle.Normal;
        GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize, fontStyle);
        style.alignment = TextAnchor.MiddleCenter;

        string text;
        ThemeSlot colorSlot;
        if (recording) {
            text = "CC_Lightweave_KeyBindingField_Recording".Translate();
            colorSlot = ThemeSlot.TextMuted;
        }
        else if (value.Key == KeyCode.None) {
            text = (string)"CC_Lightweave_KeyBindingField_Unbound".Translate();
            colorSlot = ThemeSlot.TextMuted;
        }
        else {
            text = FormatBinding(value);
            colorSlot = disabled ? ThemeSlot.TextMuted : ThemeSlot.TextPrimary;
        }

        Color saved = GUI.color;
        GUI.color = theme.GetColor(colorSlot);
        GUI.Label(RectSnap.Snap(rect), text, style);
        GUI.color = saved;
    }

    private static void DrawClearButton(Rect rect, Theme.Theme theme, Action<KeyBinding>? onChange) {
        bool hovered = Mouse.IsOver(rect);
        if (hovered) {
            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.SurfaceRaised);
            GUI.DrawTexture(RectSnap.Snap(rect), Texture2D.whiteTexture);
            GUI.color = saved;
        }

        Font font = theme.GetFont(FontRole.Body);
        int pixelSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize);
        style.alignment = TextAnchor.MiddleCenter;

        Color savedColor = GUI.color;
        GUI.color = theme.GetColor(hovered ? ThemeSlot.TextPrimary : ThemeSlot.TextMuted);
        GUI.Label(RectSnap.Snap(rect), ClearGlyph, style);
        GUI.color = savedColor;

        Event e = Event.current;
        if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
            onChange?.Invoke(new KeyBinding(KeyCode.None, KeyModifiers.None));
            e.Use();
        }
    }

    private static bool IsModifierOnly(KeyCode key) {
        return key == KeyCode.LeftControl ||
               key == KeyCode.RightControl ||
               key == KeyCode.LeftShift ||
               key == KeyCode.RightShift ||
               key == KeyCode.LeftAlt ||
               key == KeyCode.RightAlt ||
               key == KeyCode.LeftCommand ||
               key == KeyCode.RightCommand ||
               key == KeyCode.LeftWindows ||
               key == KeyCode.RightWindows ||
               key == KeyCode.None;
    }

    private static string FormatBinding(KeyBinding b) {
        StringBuilder sb = new StringBuilder();
        if ((b.Modifiers & KeyModifiers.Control) != 0) {
            sb.Append("Ctrl+");
        }

        if ((b.Modifiers & KeyModifiers.Shift) != 0) {
            sb.Append("Shift+");
        }

        if ((b.Modifiers & KeyModifiers.Alt) != 0) {
            sb.Append("Alt+");
        }

        sb.Append(b.Key.ToString());
        return sb.ToString();
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(Create(
            new KeyBinding(KeyCode.F, KeyModifiers.Control),
            _ => { },
            forced
        ));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(Create(
            new KeyBinding(KeyCode.F, KeyModifiers.Control),
            _ => { }
        ));
    }
}