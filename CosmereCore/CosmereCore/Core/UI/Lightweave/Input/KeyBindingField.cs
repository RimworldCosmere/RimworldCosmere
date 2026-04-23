using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Verse;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Input;

public readonly record struct KeyBinding(KeyCode Key, KeyModifiers Modifiers);

public static class KeyBindingField
{
    private const string ClearGlyph = "×";

    public static LightweaveNode Create(
        KeyBinding value,
        Action<KeyBinding> onChange,
        bool disabled = false,
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0)
    {
        LightweaveNode node = NodeBuilder.New("KeyBindingField", line, caller ?? string.Empty);

        node.Paint = (rect, paintChildren) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;

            Hooks.Hooks.StateHandle<bool> recording = Hooks.Hooks.UseState<bool>(false);

            bool isRecording = recording.Value && !disabled;
            InteractionState state = new InteractionState(
                Hovered: !disabled && Verse.Mouse.IsOver(rect),
                Pressed: !disabled && Verse.Mouse.IsOver(rect) && UnityEngine.Input.GetMouseButton(0),
                Focused: isRecording,
                Disabled: disabled);

            InputSurface.Draw(rect, theme, state);

            float padX = SpacingScale.Sm.ToPixels();
            float glyphSize = new Rem(1f).ToPixels();
            bool hasBinding = value.Key != KeyCode.None;
            bool showClear = hasBinding && !disabled && !isRecording;

            bool clearOnRight = dir == Direction.Ltr;
            Rect clearRect = clearOnRight
                ? new Rect(rect.xMax - padX - glyphSize, rect.y, glyphSize, rect.height)
                : new Rect(rect.x + padX, rect.y, glyphSize, rect.height);

            float leftLabelX = clearOnRight ? rect.x + padX : (showClear ? clearRect.xMax + SpacingScale.Xs.ToPixels() : rect.x + padX);
            float rightLabelX = clearOnRight
                ? (showClear ? clearRect.x - SpacingScale.Xs.ToPixels() : rect.xMax - padX)
                : rect.xMax - padX;

            Rect labelRect = new Rect(
                leftLabelX,
                rect.y,
                Mathf.Max(0f, rightLabelX - leftLabelX),
                rect.height);

            DrawLabel(labelRect, theme, value, isRecording, disabled);

            if (showClear)
            {
                DrawClearButton(clearRect, theme, onChange);
            }

            Event e = Event.current;

            if (!disabled && !isRecording
                && e.type == EventType.MouseDown && e.button == 0
                && rect.Contains(e.mousePosition)
                && !(showClear && clearRect.Contains(e.mousePosition)))
            {
                recording.Set(true);
                e.Use();
            }

            if (isRecording && e.type == EventType.MouseDown && !rect.Contains(e.mousePosition))
            {
                recording.Set(false);
            }

            if (isRecording && e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape && !e.control && !e.shift && !e.alt)
                {
                    recording.Set(false);
                    e.Use();
                }
                else if (!IsModifierOnly(e.keyCode))
                {
                    KeyModifiers mods = KeyModifiers.None;
                    if (e.control || e.command)
                    {
                        mods |= KeyModifiers.Control;
                    }
                    if (e.shift)
                    {
                        mods |= KeyModifiers.Shift;
                    }
                    if (e.alt)
                    {
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

    private static void DrawLabel(Rect rect, Theme.Theme theme, KeyBinding value, bool recording, bool disabled)
    {
        Font font = theme.GetFont(FontRole.Body);
        int pixelSize = Mathf.RoundToInt(new Rem(1f).ToPixels());
        FontStyle weight = recording ? FontStyle.Italic : FontStyle.Normal;
        GUIStyle style = GuiStyleCache.Get(font, pixelSize, weight);
        style.alignment = TextAnchor.MiddleCenter;

        string text;
        ThemeSlot colorSlot;
        if (recording)
        {
            text = (string)"CC_Lightweave_KeyBindingField_Recording".Translate();
            colorSlot = ThemeSlot.TextMuted;
        }
        else if (value.Key == KeyCode.None)
        {
            text = (string)"CC_Lightweave_KeyBindingField_Unbound".Translate();
            colorSlot = ThemeSlot.TextMuted;
        }
        else
        {
            text = FormatBinding(value);
            colorSlot = disabled ? ThemeSlot.TextMuted : ThemeSlot.TextPrimary;
        }

        Color saved = GUI.color;
        GUI.color = theme.GetColor(colorSlot);
        GUI.Label(RectSnap.Snap(rect), text, style);
        GUI.color = saved;
    }

    private static void DrawClearButton(Rect rect, Theme.Theme theme, Action<KeyBinding>? onChange)
    {
        bool hovered = Verse.Mouse.IsOver(rect);
        if (hovered)
        {
            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.SurfaceRaised);
            GUI.DrawTexture(RectSnap.Snap(rect), Texture2D.whiteTexture);
            GUI.color = saved;
        }

        Font font = theme.GetFont(FontRole.Body);
        int pixelSize = Mathf.RoundToInt(new Rem(1f).ToPixels());
        GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Normal);
        style.alignment = TextAnchor.MiddleCenter;

        Color savedColor = GUI.color;
        GUI.color = theme.GetColor(hovered ? ThemeSlot.TextPrimary : ThemeSlot.TextMuted);
        GUI.Label(RectSnap.Snap(rect), ClearGlyph, style);
        GUI.color = savedColor;

        Event e = Event.current;
        if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition))
        {
            onChange?.Invoke(new KeyBinding(KeyCode.None, KeyModifiers.None));
            e.Use();
        }
    }

    private static bool IsModifierOnly(KeyCode key)
    {
        return key == KeyCode.LeftControl
            || key == KeyCode.RightControl
            || key == KeyCode.LeftShift
            || key == KeyCode.RightShift
            || key == KeyCode.LeftAlt
            || key == KeyCode.RightAlt
            || key == KeyCode.LeftCommand
            || key == KeyCode.RightCommand
            || key == KeyCode.LeftWindows
            || key == KeyCode.RightWindows
            || key == KeyCode.None;
    }

    private static string FormatBinding(KeyBinding b)
    {
        StringBuilder sb = new StringBuilder();
        if ((b.Modifiers & KeyModifiers.Control) != 0)
        {
            sb.Append("Ctrl+");
        }
        if ((b.Modifiers & KeyModifiers.Shift) != 0)
        {
            sb.Append("Shift+");
        }
        if ((b.Modifiers & KeyModifiers.Alt) != 0)
        {
            sb.Append("Alt+");
        }
        sb.Append(b.Key.ToString());
        return sb.ToString();
    }
}
