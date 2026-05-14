using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Hooks.Hooks;

namespace Cosmere.Lightweave.Input;

[Doc(
    Id = "togglebutton",
    Summary = "Two-state button driven by a boolean value.",
    WhenToUse = "Toggle a sticky on/off state where the label conveys the action.",
    SourcePath = "Lightweave/Lightweave/Input/ToggleButton.cs"
)]
public static class ToggleButton {
    public static LightweaveNode Create(
        string label,
        bool value,
        Action<bool> onChange,
        bool disabled = false,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"ToggleButton:{label}", line, file);
        node.ApplyStyling("toggle-button", style, classes, id);
        node.PreferredHeight = new Rem(1.75f).ToPixels();

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            InteractionState state = InteractionState.Resolve(rect, null, disabled);
            ButtonVariant variant = value ? ButtonVariant.Primary : ButtonVariant.Ghost;

            ThemeSlot? bgSlot = ButtonVariants.Background(variant, state);
            ThemeSlot fgSlot = ButtonVariants.Foreground(variant, state);
            ThemeSlot? borderSlot = ButtonVariants.Border(variant, state);

            BackgroundSpec? bg = bgSlot.HasValue ? BackgroundSpec.Of(bgSlot.Value) : null;
            BorderSpec? border = borderSlot.HasValue
                ? BorderSpec.All(new Rem(1f / 16f), borderSlot.Value)
                : null;
            RadiusSpec radius = RadiusSpec.All(RadiusScale.Sm);

            PaintBox.Draw(rect, bg, border, radius);

            float overlay = ButtonVariants.OverlayAlpha(state);
            if (overlay > 0f) {
                Color overlayColor = InteractionFeedback.OverlayColor(theme, state, overlay);
                PaintBox.Draw(rect, BackgroundSpec.Of(overlayColor), null, radius);
            }

            Font font = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
            GUIStyle gstyle = GuiStyleCache.GetOrCreate(font, pixelSize);
            gstyle.alignment = TextAnchor.MiddleCenter;

            Color fg = theme.GetColor(fgSlot);
            Color savedColor = GUI.color;
            GUI.color = fg;
            GUI.Label(RectSnap.Snap(rect), label, gstyle);
            GUI.color = savedColor;

            paintChildren();

            Event e = Event.current;
            if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
                onChange?.Invoke(!value);
                e.Use();
            }
        };

        return node;
    }

    [DocVariant("CL_Playground_Label_On")]
    public static DocSample DocsOn() {
        StateHandle<bool> onValue = UseState(true);
        return new DocSample(() => Create("On", onValue.Value, v => onValue.Set(v)));
    }

    [DocVariant("CL_Playground_Label_Off")]
    public static DocSample DocsOff() {
        StateHandle<bool> offValue = UseState(false);
        return new DocSample(() => Create("Off", offValue.Value, v => offValue.Set(v)));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        StateHandle<bool> value = UseState(false);
        return new DocSample(() => Create("Toggle", value.Value, v => value.Set(v)));
    }
}
