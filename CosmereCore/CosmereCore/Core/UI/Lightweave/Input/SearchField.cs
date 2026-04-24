using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Input;

public static class SearchField {
    private const string ClearGlyph = "×";

    public static LightweaveNode Create(
        string value,
        Action<string> onChange,
        string? placeholder = null,
        bool disabled = false,
        object? instanceKey = null,
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0
    ) {
        string callerFile = caller ?? string.Empty;
        int callerLine = line;
        string keySuffix = instanceKey == null ? string.Empty : "#" + instanceKey;
        string focusKey = callerFile + "#sf_focus" + keySuffix;
        string bufferKey = callerFile + "#sf_buffer" + keySuffix;
        string syncedFromKey = callerFile + "#sf_syncedFrom" + keySuffix;

        LightweaveNode node = NodeBuilder.New("SearchField", callerLine, callerFile);
        node.PreferredHeight = new Rem(1.75f).ToPixels();

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;

            Hooks.Hooks.RefHandle<string> focusNameRef = Hooks.Hooks.UseRef<string>("", callerLine, focusKey);
            if (string.IsNullOrEmpty(focusNameRef.Current)) {
                focusNameRef.Current = "lw_sf_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            string focusName = focusNameRef.Current;

            Hooks.Hooks.StateHandle<string> buffer = Hooks.Hooks.UseState(value ?? string.Empty, callerLine, bufferKey);
            Hooks.Hooks.RefHandle<string> syncedFrom = Hooks.Hooks.UseRef(
                value ?? string.Empty,
                callerLine,
                syncedFromKey
            );

            string incoming = value ?? string.Empty;
            bool isFocusedThisFrame = GUI.GetNameOfFocusedControl() == focusName;
            if (!isFocusedThisFrame && !string.Equals(syncedFrom.Current, incoming, StringComparison.Ordinal)) {
                buffer.Set(incoming);
                syncedFrom.Current = incoming;
            }

            InteractionState state = InteractionState.Resolve(rect, focusName, disabled);
            InputSurface.Draw(rect, theme, state);

            float padX = InputSurface.PaddingX.ToPixels();
            float glyphSize = new Rem(1f).ToPixels();
            bool hasValue = !string.IsNullOrEmpty(buffer.Value);

            bool glyphOnLeft = dir == Direction.Ltr;
            Rect glyphRect = glyphOnLeft
                ? new Rect(rect.x + padX, rect.y, glyphSize, rect.height)
                : new Rect(rect.xMax - padX - glyphSize, rect.y, glyphSize, rect.height);

            Rect clearRect = glyphOnLeft
                ? new Rect(rect.xMax - padX - glyphSize, rect.y, glyphSize, rect.height)
                : new Rect(rect.x + padX, rect.y, glyphSize, rect.height);

            float leftContentX = glyphOnLeft ? glyphRect.xMax + SpacingScale.Xs.ToPixels() : rect.x + padX;
            float rightContentX = glyphOnLeft
                ? hasValue && !disabled ? clearRect.x - SpacingScale.Xs.ToPixels() : rect.xMax - padX
                : glyphRect.x - SpacingScale.Xs.ToPixels();

            if (!glyphOnLeft && hasValue && !disabled) {
                leftContentX = clearRect.xMax + SpacingScale.Xs.ToPixels();
            }

            Rect inner = new Rect(
                leftContentX,
                rect.y,
                Mathf.Max(0f, rightContentX - leftContentX),
                rect.height
            );

            if (!hasValue) {
                DrawMagnifier(glyphRect, theme, ThemeSlot.TextMuted);
            } else {
                DrawMagnifier(glyphRect, theme, ThemeSlot.TextSecondary);
            }

            bool showPlaceholder = !state.Focused && !hasValue && !string.IsNullOrEmpty(placeholder);

            if (showPlaceholder) {
                Font phFont = theme.GetFont(FontRole.Body);
                int phSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
                GUIStyle phStyle = GuiStyleCache.Get(phFont, phSize);
                phStyle.alignment = glyphOnLeft ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
                Color savedColor = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextMuted);
                GUI.Label(RectSnap.Snap(inner), placeholder, phStyle);
                GUI.color = savedColor;
            }

            if (disabled) {
                Font roFont = theme.GetFont(FontRole.Body);
                int roSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
                GUIStyle roStyle = GuiStyleCache.Get(roFont, roSize);
                roStyle.alignment = glyphOnLeft ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
                Color savedColor = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextMuted);
                GUI.Label(RectSnap.Snap(inner), buffer.Value ?? string.Empty, roStyle);
                GUI.color = savedColor;
            } else {
                Font sfFont = theme.GetFont(FontRole.Body);
                int sfSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
                Color sfTextColor = theme.GetColor(ThemeSlot.TextPrimary);
                GUIStyle sfStyle = InputSurface.GetChromelessTextFieldStyle(sfFont, sfSize, sfTextColor);
                GUI.SetNextControlName(focusName);
                string next = GUI.TextField(RectSnap.Snap(inner), buffer.Value ?? string.Empty, sfStyle);
                if (next != buffer.Value) {
                    buffer.Set(next);
                    syncedFrom.Current = next;
                    onChange?.Invoke(next);
                }

                if (hasValue) {
                    DrawClearButton(clearRect, theme, onChange, buffer, syncedFrom);
                }
            }

            Event evt = Event.current;
            if (!disabled && evt.type == EventType.MouseDown && evt.button == 0 && !rect.Contains(evt.mousePosition)) {
                if (GUI.GetNameOfFocusedControl() == focusName) {
                    GUI.FocusControl(null);
                }
            }

            paintChildren();
        };

        return node;
    }

    private static void DrawGlyph(Rect rect, string glyph, Theme.Theme theme, ThemeSlot slot) {
        Font font = theme.GetFont(FontRole.Body);
        int pixelSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
        GUIStyle style = GuiStyleCache.Get(font, pixelSize);
        style.alignment = TextAnchor.MiddleCenter;

        Color saved = GUI.color;
        GUI.color = theme.GetColor(slot);
        GUI.Label(RectSnap.Snap(rect), glyph, style);
        GUI.color = saved;
    }

    private static void DrawMagnifier(Rect rect, Theme.Theme theme, ThemeSlot slot) {
        Texture2D? tex = TexButton.Search;
        if (tex == null) {
            return;
        }

        float size = Mathf.Min(rect.width, rect.height);
        Rect iconRect = new Rect(
            rect.x + (rect.width - size) * 0.5f,
            rect.y + (rect.height - size) * 0.5f,
            size,
            size
        );
        Color saved = GUI.color;
        GUI.color = theme.GetColor(slot);
        GUI.DrawTexture(RectSnap.Snap(iconRect), tex);
        GUI.color = saved;
    }

    private static void DrawClearButton(
        Rect rect,
        Theme.Theme theme,
        Action<string>? onChange,
        Hooks.Hooks.StateHandle<string> buffer,
        Hooks.Hooks.RefHandle<string> syncedFrom
    ) {
        bool hovered = Mouse.IsOver(rect);
        if (hovered) {
            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.SurfaceRaised);
            GUI.DrawTexture(RectSnap.Snap(rect), Texture2D.whiteTexture);
            GUI.color = saved;
        }

        ThemeSlot glyphSlot = hovered ? ThemeSlot.TextPrimary : ThemeSlot.TextMuted;
        DrawGlyph(rect, ClearGlyph, theme, glyphSlot);

        Event e = Event.current;
        if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
            buffer.Set(string.Empty);
            syncedFrom.Current = string.Empty;
            onChange?.Invoke(string.Empty);
            e.Use();
        }
    }
}