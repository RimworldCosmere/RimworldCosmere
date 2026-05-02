using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Input;

[Doc(
    Id = "searchfield",
    Summary = "Single-line text input with leading magnifier and trailing clear glyph.",
    WhenToUse = "Filter a list or trigger search-as-you-type.",
    SourcePath = "Lightweave/Lightweave/Input/SearchField.cs"
)]
public static class SearchField {
    private const string ClearGlyph = "×";

    public static LightweaveNode Create(
        [DocParam("Current query text.")]
        string value,
        [DocParam("Invoked with the new query on every change.")]
        Action<string> onChange,
        [DocParam("Optional placeholder rendered when the buffer is empty.")]
        string? placeholder = null,
        [DocParam("Disables interaction and applies disabled styling.")]
        bool disabled = false,
        [DocParam("Optional key disambiguating multiple instances declared on the same line.")]
        object? instanceKey = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        string keySuffix = instanceKey == null ? string.Empty : "#" + instanceKey;
        string focusKey = file + "#sf_focus" + keySuffix;
        string bufferKey = file + "#sf_buffer" + keySuffix;
        string syncedFromKey = file + "#sf_syncedFrom" + keySuffix;

        LightweaveNode node = NodeBuilder.New("SearchField", line, file);
        node.PreferredHeight = new Rem(1.75f).ToPixels();

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;

            Hooks.Hooks.RefHandle<string> focusNameRef = Hooks.Hooks.UseRef<string>("", line, focusKey);
            if (string.IsNullOrEmpty(focusNameRef.Current)) {
                focusNameRef.Current = "lw_sf_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            string focusName = focusNameRef.Current;

            Hooks.Hooks.StateHandle<string> buffer = Hooks.Hooks.UseState(value ?? string.Empty, line, bufferKey);
            Hooks.Hooks.RefHandle<string> syncedFrom = Hooks.Hooks.UseRef(
                value ?? string.Empty,
                line,
                syncedFromKey
            );

            string incoming = value ?? string.Empty;
            bool isFocusedThisFrame = GUI.GetNameOfFocusedControl() == focusName;
            if (!isFocusedThisFrame && !string.Equals(syncedFrom.Current, incoming, StringComparison.Ordinal)) {
                buffer.Set(incoming);
                syncedFrom.Current = incoming;
            }

            InteractionState state = InteractionState.Resolve(rect, focusName, disabled);
            InputSurface.Draw(rect, state);

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

            TextAnchor textAnchor = glyphOnLeft ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;

            if (showPlaceholder) {
                InputSurface.DrawPlaceholder(inner, placeholder, theme, textAnchor);
            }

            if (disabled) {
                InputSurface.DrawReadOnlyValue(inner, buffer.Value ?? string.Empty, theme, textAnchor);
            } else {
                Font sfFont = theme.GetFont(FontRole.Body);
                int sfSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
                Color sfTextColor = theme.GetColor(ThemeSlot.TextPrimary);
                GUIStyle sfStyle = InputSurface.ConfigureChromelessTextFieldStyle(sfFont, sfSize, sfTextColor);
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

    [DocVariant("CC_Playground_Label_Empty")]
    public static DocSample DocsEmpty() {
        bool forced = PlaygroundDemoContext.Current.ForceDisabled;
        return new DocSample(Create(
            string.Empty,
            _ => { },
            (string)"CC_Playground_SearchField_Placeholder".Translate(),
            forced
        ));
    }

    [DocVariant("CC_Playground_Label_Filled")]
    public static DocSample DocsFilled() {
        bool forced = PlaygroundDemoContext.Current.ForceDisabled;
        return new DocSample(Create("highstorm", _ => { }, disabled: forced));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(Create("highstorm", _ => { }));
    }
}