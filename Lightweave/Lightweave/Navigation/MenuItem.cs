using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cosmere.Lightweave.Typography;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Lightweave.Navigation;

[Doc(
    Id = "menu-item",
    Summary = "Standalone clickable menu-style row with hover background, optional leading icon, and danger styling.",
    WhenToUse = "Composing custom popovers / drawer panels / sidebars that need menu-item visuals without the full Menu widget.",
    SourcePath = "Lightweave/Lightweave/Navigation/MenuItem.cs"
)]
public static class MenuItem {
    public static LightweaveNode Create(
        [DocParam("Display label text.")]
        string label,
        [DocParam("Click handler.")]
        Action? onClick = null,
        [DocParam("Optional leading icon node.")]
        LightweaveNode? icon = null,
        [DocParam("Render in danger color (e.g. destructive actions like Quit).")]
        bool danger = false,
        [DocParam("Disable interaction.")]
        bool disabled = false,
        [DocParam("Row height in rems.")]
        float heightRem = 1.65f,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("MenuItem:" + label, line, file);
        node.ApplyStyling("menu-item", style, classes, id);
        node.PreferredHeight = new Rem(heightRem).ToPixels();
        if (icon != null) {
            node.Children.Add(icon);
        }

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            InteractionState st = InteractionState.Resolve(rect, null, disabled);
            bool hot = !disabled && (st.Hovered || st.Pressed);

            if (hot) {
                Color hover = theme.GetColor(ThemeSlot.SurfaceRaised);
                hover.a = 0.55f;
                PaintBox.Draw(rect, BackgroundSpec.Of(hover), null, RadiusSpec.All(RadiusScale.Xs));
            }

            float padPx = new Rem(0.4f).ToPixels();
            float gapPx = new Rem(0.5f).ToPixels();
            float iconPx = new Rem(1.0f).ToPixels();
            float labelStartX = rect.x + padPx;

            if (icon != null) {
                Rect iconRect = new Rect(rect.x + padPx, rect.y + (rect.height - iconPx) * 0.5f, iconPx, iconPx);
                icon.MeasuredRect = iconRect;
                labelStartX = iconRect.xMax + gapPx;
            }

            int px = Mathf.RoundToInt(new Rem(0.78f).ToFontPx());
            FontStyle weight = danger ? FontStyle.Bold : FontStyle.Normal;
            GUIStyle style = GuiStyleCache.GetOrCreate(theme, FontRole.Body, px, weight);
            style.alignment = TextAnchor.MiddleLeft;
            style.clipping = TextClipping.Clip;

            ThemeSlot slot;
            if (disabled) {
                slot = ThemeSlot.TextMuted;
            }
            else if (danger) {
                slot = hot ? ThemeSlot.TextPrimary : ThemeSlot.StatusDanger;
            }
            else {
                slot = hot ? ThemeSlot.TextPrimary : ThemeSlot.TextSecondary;
            }

            Color saved = GUI.color;
            GUI.color = theme.GetColor(slot);
            Rect labelRect = new Rect(labelStartX, rect.y, rect.xMax - padPx - labelStartX, rect.height);
            GUI.Label(RectSnap.Snap(labelRect), label, style);
            GUI.color = saved;

            paintChildren();

            if (!disabled && onClick != null) {
                MouseoverSounds.DoRegion(rect);
                Event e = Event.current;
                if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
                    onClick.Invoke();
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    e.Use();
                }
            }
        };
        return node;
    }

    [DocVariant("CL_Playground_Navigation_MenuItem_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => MenuItem.Create("Open settings", onClick: () => { }));
    }

    [DocVariant("CL_Playground_Navigation_MenuItem_Danger", Order = 1)]
    public static DocSample DocsDanger() {
        return new DocSample(() => MenuItem.Create("Quit to desktop", onClick: () => { }, danger: true));
    }

    [DocVariant("CL_Playground_Navigation_MenuItem_Disabled", Order = 2)]
    public static DocSample DocsDisabled() {
        return new DocSample(() => MenuItem.Create("Coming soon", onClick: () => { }, disabled: true));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => MenuItem.Create("Documentation", onClick: () => { }));
    }
}
