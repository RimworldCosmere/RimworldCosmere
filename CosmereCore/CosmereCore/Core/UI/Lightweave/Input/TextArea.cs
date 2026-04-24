using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Input;

public static class TextArea {
    public static LightweaveNode Create(
        string value,
        Action<string> onChange,
        string? placeholder = null,
        int minRows = 3,
        int maxRows = 8,
        bool readOnly = false,
        bool disabled = false,
        object? instanceKey = null,
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0
    ) {
        string callerFile = caller ?? string.Empty;
        int callerLine = line;
        string keySuffix = instanceKey == null ? string.Empty : "#" + instanceKey;
        string focusKey = callerFile + "#ta_focus" + keySuffix;
        string bufferKey = callerFile + "#ta_buffer" + keySuffix;
        string wasFocusedKey = callerFile + "#ta_wasFocused" + keySuffix;

        LightweaveNode node = NodeBuilder.New("TextArea", callerLine, callerFile);
        float lineHeightPx = new Rem(1.5f).ToPixels();
        int initialRows = Mathf.Clamp(
            CountRows(value ?? string.Empty),
            Mathf.Max(1, minRows),
            Mathf.Max(minRows, maxRows)
        );
        node.PreferredHeight = initialRows * lineHeightPx;

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;

            Hooks.Hooks.RefHandle<string> focusNameRef = Hooks.Hooks.UseRef<string>("", callerLine, focusKey);
            if (string.IsNullOrEmpty(focusNameRef.Current)) {
                focusNameRef.Current = "lw_ta_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            string focusName = focusNameRef.Current;

            Hooks.Hooks.StateHandle<string> buffer = Hooks.Hooks.UseState(value ?? string.Empty, callerLine, bufferKey);
            Hooks.Hooks.RefHandle<bool> wasFocused = Hooks.Hooks.UseRef(false, callerLine, wasFocusedKey);

            float lineHeight = new Rem(1.5f).ToPixels();
            int contentRows = CountRows(buffer.Value ?? string.Empty);
            int clampedRows = Mathf.Clamp(contentRows, Mathf.Max(1, minRows), Mathf.Max(minRows, maxRows));
            float resolvedHeight = clampedRows * lineHeight;
            Rect surfaceRect = new Rect(rect.x, rect.y, rect.width, resolvedHeight);

            InteractionState state = InteractionState.Resolve(surfaceRect, focusName, disabled);
            InputSurface.Draw(surfaceRect, theme, state);

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
                Font phFont = theme.GetFont(FontRole.Body);
                int phSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
                GUIStyle phStyle = GuiStyleCache.Get(phFont, phSize);
                phStyle.alignment = TextAnchor.UpperLeft;
                Color savedColor = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextMuted);
                GUI.Label(RectSnap.Snap(inner), placeholder, phStyle);
                GUI.color = savedColor;
            }

            if (disabled) {
                Font roFont = theme.GetFont(FontRole.Body);
                int roSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
                GUIStyle roStyle = GuiStyleCache.Get(roFont, roSize);
                roStyle.alignment = TextAnchor.UpperLeft;
                Color savedColor = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextMuted);
                GUI.Label(RectSnap.Snap(inner), buffer.Value ?? string.Empty, roStyle);
                GUI.color = savedColor;
            } else {
                Font taFont = theme.GetFont(FontRole.Body);
                int taSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
                Color taTextColor = theme.GetColor(ThemeSlot.TextPrimary);
                GUIStyle taStyle = InputSurface.GetChromelessTextAreaStyle(taFont, taSize, taTextColor);
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
}