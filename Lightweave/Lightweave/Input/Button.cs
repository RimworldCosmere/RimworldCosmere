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
        [DocParam("Text rendered inside the button. Ignored when body is provided.")]
        string label,
        [DocParam("Action invoked on left mouse up while hovering.")]
        Action? onClick,
        [DocParam("Visual variant: Primary, Secondary, Ghost, Danger, or Dock.")]
        ButtonVariant variant = ButtonVariant.Primary,
        [DocParam("Optional node painted on the leading edge.")]
        LightweaveNode? leading = null,
        [DocParam("Optional node painted on the trailing edge.")]
        LightweaveNode? trailing = null,
        [DocParam("Override foreground color for the label.")]
        ColorRef? foregroundOverride = null,
        [DocParam("Disables interaction and applies disabled styling.")]
        bool disabled = false,
        [DocParam("When true, button stretches to fill the allocated width. Default sizes to label + padding.")]
        bool fullWidth = false,
        [DocParam("When true, button stretches to fill the allocated height. Default uses an intrinsic 1.75rem (or `height` if set).")]
        bool fillHeight = false,
        [DocParam("Override hover sound. Null = component default (true).")]
        bool? playHoverSound = null,
        [DocParam("Optional content node painted inside the button instead of the label. Used for non-text bodies (e.g. dock tiles with stacked label + hotkey).")]
        LightweaveNode? body = null,
        [DocParam("Override intrinsic height. Null = 1.75rem.")]
        Rem? height = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Button:{label}", line, file);
        float intrinsicHeightPx = (height ?? new Rem(1.75f)).ToPixels();
        node.PreferredHeight = intrinsicHeightPx;

        if (leading != null) {
            node.Children.Add(leading);
        }

        if (trailing != null) {
            node.Children.Add(trailing);
        }

        if (body != null) {
            node.Children.Add(body);
        }

        node.Paint = (allocatedRect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;

            float desiredHeight = intrinsicHeightPx;
            float h = fillHeight ? allocatedRect.height : Mathf.Min(desiredHeight, allocatedRect.height);
            float yOffset = fillHeight ? 0f : (allocatedRect.height - h) * 0.5f;

            float padPx = SpacingScale.Sm.ToPixels();
            float iconSize = Mathf.Min(h - padPx, new Rem(1.25f).ToPixels());

            Font font = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleCenter;

            float labelWidth = string.IsNullOrEmpty(label) || body != null
                ? 0f
                : style.CalcSize(new GUIContent(label)).x;
            float iconAllowance = (leading != null ? iconSize + padPx : 0f)
                                + (trailing != null ? iconSize + padPx : 0f);
            float naturalWidth = labelWidth + iconAllowance + padPx * 2f;

            Rect rect;
            if (fullWidth || body != null) {
                rect = new Rect(allocatedRect.x, allocatedRect.y + yOffset, allocatedRect.width, h);
            }
            else {
                float w = Mathf.Min(naturalWidth, allocatedRect.width);
                float x = dir == Direction.Rtl ? allocatedRect.xMax - w : allocatedRect.x;
                rect = new Rect(x, allocatedRect.y + yOffset, w, h);
            }

            node.MeasuredRect = rect;

            InteractionState state = InteractionState.Resolve(rect, null, disabled);

            ThemeSlot fgSlot = ButtonVariants.Foreground(variant, state);
            ThemeSlot? borderSlot = ButtonVariants.Border(variant, state);
            RadiusSpec radius = RadiusSpec.All(RadiusScale.Sm);
            BorderSpec? border = borderSlot.HasValue
                ? BorderSpec.All(new Rem(1f / 16f), borderSlot.Value)
                : null;

            if (variant == ButtonVariant.Dock) {
                bool active = state.Hovered || state.Pressed;
                BackdropBlur.Draw(rect, active ? 8f : 6f);
                Color translucent = new Color(20f / 255f, 16f / 255f, 11f / 255f, active ? 0.88f : 0.78f);
                PaintBox.Draw(rect, BackgroundSpec.Of(translucent), border, radius);
            }
            else {
                ThemeSlot bgSlot = ButtonVariants.Background(variant, state);
                BackgroundSpec bg;
                if (variant == ButtonVariant.Primary && !state.Pressed && !disabled) {
                    Color top = theme.GetColor(bgSlot);
                    Color.RGBToHSV(top, out float hue, out float sat, out float val);
                    Color bottom = Color.HSVToRGB(hue, sat, val * 0.78f);
                    bottom.a = top.a;
                    bg = new BackgroundSpec.Gradient(GradientTextureCache.Vertical(top, bottom));
                }
                else {
                    bg = BackgroundSpec.Of(bgSlot);
                }

                PaintBox.Draw(rect, bg, border, radius);
            }

            float overlay = ButtonVariants.OverlayAlpha(state);
            if (overlay > 0f) {
                Color overlayColor = InteractionFeedback.OverlayColor(theme, state, overlay);
                PaintBox.Draw(rect, BackgroundSpec.Of(overlayColor), null, radius);
            }

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
                }
                else {
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
                }
                else {
                    labelRect = new Rect(labelRect.x, labelRect.y, trailingX - padPx - labelRect.x, labelRect.height);
                }
            }

            if (body != null) {
                body.MeasuredRect = rect;
                LightweaveRoot.PaintSubtree(body, rect);
            }
            else {
                Color fg = foregroundOverride switch {
                    ColorRef.Literal lit => lit.Value,
                    ColorRef.Token tok => theme.GetColor(tok.Slot),
                    _ => theme.GetColor(fgSlot),
                };

                Color savedColor = GUI.color;
                GUI.color = fg;
                GUI.Label(RectSnap.Snap(labelRect), label, style);
                GUI.color = savedColor;
            }

            paintChildren();

            InteractionFeedback.Apply(rect, !disabled, playHoverSound ?? true);

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

    [DocVariant("CL_Playground_Label_Primary")]
    public static DocSample DocsPrimary() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(() => Create("Primary", () => { }, disabled: forced));
    }

    [DocVariant("CL_Playground_Label_Secondary")]
    public static DocSample DocsSecondary() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(() => Create("Secondary", () => { }, ButtonVariant.Secondary, disabled: forced));
    }

    [DocVariant("CL_Playground_Label_Ghost")]
    public static DocSample DocsGhost() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(() => Create("Ghost", () => { }, ButtonVariant.Ghost, disabled: forced));
    }

    [DocVariant("CL_Playground_Label_Danger")]
    public static DocSample DocsDanger() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(() => Create("Danger", () => { }, ButtonVariant.Danger, disabled: forced));
    }

    [DocState("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(() => Create("Default", () => { }, disabled: forced));
    }

    [DocState("CL_Playground_Label_Hover")]
    public static DocSample DocsHover() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(() => Create("Hover me", () => { }, disabled: forced));
    }

    [DocState("CL_Playground_Label_Disabled")]
    public static DocSample DocsDisabled() {
        return new DocSample(() => Create("Disabled", () => { }, disabled: true));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => Create("Confirm", () => { }));
    }
}
