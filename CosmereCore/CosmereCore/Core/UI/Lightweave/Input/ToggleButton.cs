using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Input;

public static class ToggleButton
{
    public static LightweaveNode Create(
        string label,
        bool value,
        Action<bool> onChange,
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0)
    {
        LightweaveNode node = NodeBuilder.New($"ToggleButton:{label}", line, caller ?? string.Empty);

        node.Paint = (rect, paintChildren) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;
            InteractionState state = InteractionState.Resolve(rect, focusName: null, disabled: false);
            ButtonVariant variant = value ? ButtonVariant.Primary : ButtonVariant.Ghost;

            ThemeSlot bgSlot = ButtonVariants.Background(variant, state);
            ThemeSlot fgSlot = ButtonVariants.Foreground(variant, state);
            ThemeSlot? borderSlot = ButtonVariants.Border(variant, state);

            BackgroundSpec bg = new BackgroundSpec.Solid(bgSlot);
            BorderSpec? border = borderSlot.HasValue
                ? (BorderSpec?)BorderSpec.All(new Rem(1f / 16f), borderSlot.Value)
                : null;
            RadiusSpec radius = RadiusSpec.All(new Rem(0.25f));

            PaintBox.Draw(rect, bg, border, radius);

            float overlay = ButtonVariants.OverlayAlpha(state);
            if (overlay > 0f)
            {
                Color overlayColor = state.Pressed
                    ? new Color(0f, 0f, 0f, overlay)
                    : new Color(1f, 1f, 1f, overlay);
                PaintBox.Draw(rect, new BackgroundSpec.Solid(overlayColor), null, radius);
            }

            Font font = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToPixels());
            GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleCenter;

            Color fg = theme.GetColor(fgSlot);
            Color savedColor = GUI.color;
            GUI.color = fg;
            GUI.Label(RectSnap.Snap(rect), label, style);
            GUI.color = savedColor;

            paintChildren();

            Event e = Event.current;
            if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition))
            {
                onChange?.Invoke(!value);
                e.Use();
            }
        };

        return node;
    }
}
