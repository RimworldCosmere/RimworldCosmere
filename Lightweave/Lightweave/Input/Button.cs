using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.Input;

[Doc(
    Id = "button",
    Summary = "Clickable text button with variant styling.",
    WhenToUse = "Trigger an action with a label-driven affordance.",
    SourcePath = "Lightweave/Lightweave/Input/Button.cs"
)]
public static class Button {
    public static LightweaveNode Create(
        [DocParam("Text rendered inside the button.")]
        string label,
        [DocParam("Action invoked on left mouse up while hovering.")]
        Action? onClick,
        [DocParam("Visual variant: Primary, Secondary, Ghost, or Danger.")]
        ButtonVariant variant = ButtonVariant.Primary,
        [DocParam("Optional node painted on the leading edge.")]
        LightweaveNode? leading = null,
        [DocParam("Optional node painted on the trailing edge.")]
        LightweaveNode? trailing = null,
        [DocParam("Override foreground color for the label.")]
        ColorRef? foregroundOverride = null,
        [DocParam("Disables interaction and applies disabled styling.")]
        bool disabled = false,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Button:{label}", line, file);
        node.PreferredHeight = new Rem(1.75f).ToPixels();

        if (leading != null) {
            node.Children.Add(leading);
        }

        if (trailing != null) {
            node.Children.Add(trailing);
        }

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            InteractionState state = InteractionState.Resolve(rect, null, disabled);

            ThemeSlot bgSlot = ButtonVariants.Background(variant, state);
            ThemeSlot fgSlot = ButtonVariants.Foreground(variant, state);
            ThemeSlot? borderSlot = ButtonVariants.Border(variant, state);

            BackgroundSpec bg = new BackgroundSpec.Solid(bgSlot);
            BorderSpec? border = borderSlot.HasValue
                ? BorderSpec.All(new Rem(1f / 16f), borderSlot.Value)
                : null;
            RadiusSpec radius = RadiusSpec.All(new Rem(0.25f));

            PaintBox.Draw(rect, bg, border, radius);

            float overlay = ButtonVariants.OverlayAlpha(state);
            if (overlay > 0f) {
                Color overlayColor = state.Pressed
                    ? new Color(0f, 0f, 0f, overlay)
                    : new Color(1f, 1f, 1f, overlay);
                PaintBox.Draw(rect, new BackgroundSpec.Solid(overlayColor), null, radius);
            }

            float padPx = SpacingScale.Sm.ToPixels();
            float iconSize = Mathf.Min(rect.height - padPx, new Rem(1.25f).ToPixels());
            bool rtl = dir == Direction.Rtl;

            Rect labelRect = new Rect(rect.x + padPx, rect.y, rect.width - padPx * 2f, rect.height);

            if (leading != null) {
                float leadingX = rtl
                    ? rect.xMax - padPx - iconSize
                    : rect.x + padPx;
                Rect leadingRect = new Rect(leadingX, rect.y + (rect.height - iconSize) / 2f, iconSize, iconSize);
                leading.MeasuredRect = leadingRect;
                if (rtl) {
                    labelRect = new Rect(
                        labelRect.x,
                        labelRect.y,
                        labelRect.width - (iconSize + padPx),
                        labelRect.height
                    );
                } else {
                    labelRect = new Rect(
                        leadingX + iconSize + padPx,
                        labelRect.y,
                        rect.xMax - padPx - (leadingX + iconSize + padPx),
                        labelRect.height
                    );
                }
            }

            if (trailing != null) {
                float trailingX = rtl
                    ? rect.x + padPx
                    : rect.xMax - padPx - iconSize;
                Rect trailingRect = new Rect(trailingX, rect.y + (rect.height - iconSize) / 2f, iconSize, iconSize);
                trailing.MeasuredRect = trailingRect;
                if (rtl) {
                    labelRect = new Rect(
                        trailingX + iconSize + padPx,
                        labelRect.y,
                        labelRect.xMax - (trailingX + iconSize + padPx),
                        labelRect.height
                    );
                } else {
                    labelRect = new Rect(labelRect.x, labelRect.y, trailingX - padPx - labelRect.x, labelRect.height);
                }
            }

            Font font = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
            GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleCenter;

            Color fg = foregroundOverride switch {
                ColorRef.Literal lit => lit.Value,
                ColorRef.Token tok => theme.GetColor(tok.Slot),
                _ => theme.GetColor(fgSlot),
            };

            Color savedColor = GUI.color;
            GUI.color = fg;
            GUI.Label(RectSnap.Snap(labelRect), label, style);
            GUI.color = savedColor;

            paintChildren();


            Event e = Event.current;
            if (!disabled &&
                onClick != null &&
                e.type == EventType.MouseUp &&
                e.button == 0 &&
                rect.Contains(e.mousePosition)) {
                onClick.Invoke();
                e.Use();
            }
        };

        return node;
    }

    [DocVariant("CC_Playground_Label_Primary")]
    public static DocSample DocsPrimary() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(Create("Primary", () => { }, disabled: forced));
    }

    [DocVariant("CC_Playground_Label_Secondary")]
    public static DocSample DocsSecondary() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(Create("Secondary", () => { }, ButtonVariant.Secondary, disabled: forced));
    }

    [DocVariant("CC_Playground_Label_Ghost")]
    public static DocSample DocsGhost() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(Create("Ghost", () => { }, ButtonVariant.Ghost, disabled: forced));
    }

    [DocVariant("CC_Playground_Label_Danger")]
    public static DocSample DocsDanger() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(Create("Danger", () => { }, ButtonVariant.Danger, disabled: forced));
    }

    [DocState("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(Create("Default", () => { }, disabled: forced));
    }

    [DocState("CC_Playground_Label_Hover")]
    public static DocSample DocsHover() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(Create("Hover me", () => { }, disabled: forced));
    }

    [DocState("CC_Playground_Label_Disabled")]
    public static DocSample DocsDisabled() {
        return new DocSample(Create("Disabled", () => { }, disabled: true));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(Create("Confirm", () => { }));
    }
}
