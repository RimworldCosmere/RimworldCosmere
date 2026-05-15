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
        string label,
        Action? onClick,
        ButtonVariant variant = ButtonVariant.Primary,
        bool ghost = false,
        LightweaveNode? leading = null,
        LightweaveNode? trailing = null,
        bool disabled = false,
        bool? playHoverSound = null,
        LightweaveNode? body = null,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Button:{label}", line, file);
        node.ApplyStyling("button", style, classes, id);

        Style resolved0 = node.GetResolvedStyle();
        float intrinsicHeightPx = resolved0.Height is { Mode: Length.Kind.Rem } h0
            ? h0.ToPixels(0f, 0f)
            : new Rem(2.875f).ToPixels();
        node.PreferredHeight = intrinsicHeightPx;

        node.MeasureWidth = () => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
            GUIStyle gstyle = GuiStyleCache.GetOrCreate(font, pixelSize);
            float labelWidth = string.IsNullOrEmpty(label) || body != null
                ? 0f
                : gstyle.CalcSize(new GUIContent(label)).x;
            float padXPx = new Rem(2f).ToPixels();
            float padYPx = new Rem(1f).ToPixels();
            float iconGapPx = SpacingScale.Sm.ToPixels();
            float iconSize = Mathf.Min(intrinsicHeightPx - padYPx, new Rem(1.25f).ToPixels());
            float iconAllowance = (leading != null ? iconSize + iconGapPx : 0f)
                                + (trailing != null ? iconSize + iconGapPx : 0f);
            return labelWidth + iconAllowance + padXPx * 2f;
        };

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
            Style s = node.GetResolvedStyle();

            bool fullWidth = s.Width is { IsGrower: true };
            bool fillHeight = s.Height is { IsGrower: true };

            float desiredHeight = intrinsicHeightPx;
            float h = fillHeight ? allocatedRect.height : Mathf.Min(desiredHeight, allocatedRect.height);
            float yOffset = fillHeight ? 0f : (allocatedRect.height - h) * 0.5f;

            float padXPx = new Rem(2f).ToPixels();
            float padYPx = new Rem(1f).ToPixels();
            float iconGapPx = SpacingScale.Sm.ToPixels();
            float iconSize = Mathf.Min(h - padYPx, new Rem(1.25f).ToPixels());

            Font font = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
            GUIStyle gstyle = GuiStyleCache.GetOrCreate(font, pixelSize);
            gstyle.alignment = TextAnchor.MiddleCenter;

            float labelWidth = string.IsNullOrEmpty(label) || body != null
                ? 0f
                : gstyle.CalcSize(new GUIContent(label)).x;
            float iconAllowance = (leading != null ? iconSize + iconGapPx : 0f)
                                + (trailing != null ? iconSize + iconGapPx : 0f);
            float naturalWidth = labelWidth + iconAllowance + padXPx * 2f;

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

            ThemeSlot fgSlot = ButtonVariants.Foreground(variant, state, ghost);
            ThemeSlot? borderSlot = ButtonVariants.Border(variant, state, ghost);
            RadiusSpec radius = RadiusSpec.All(RadiusScale.Sm);
            BorderSpec? border = borderSlot.HasValue
                ? BorderSpec.All(new Rem(1f / 16f), borderSlot.Value)
                : null;

            if (variant == ButtonVariant.Frosted && !ghost) {
                bool active = state.Hovered || state.Pressed;
                BackdropBlur.Draw(rect, active ? 8f : 6f);
                Color translucent = new Color(20f / 255f, 16f / 255f, 11f / 255f, active ? 0.88f : 0.78f);
                PaintBox.Draw(rect, BackgroundSpec.Of(translucent), border, radius);
            }
            else {
                ThemeSlot? bgSlot = ButtonVariants.Background(variant, state, ghost);
                BackgroundSpec? bg;
                if (!bgSlot.HasValue) {
                    if (variant == ButtonVariant.Ghost && state.Hovered && !disabled) {
                        bg = BackgroundSpec.Of(new Color(0.157f, 0.125f, 0.086f, 0.4f));
                    }
                    else {
                        bg = null;
                    }
                }
                else if (variant == ButtonVariant.Primary && !ghost && !state.Pressed && !disabled) {
                    Color top = theme.GetColor(bgSlot.Value);
                    Color.RGBToHSV(top, out float hue, out float sat, out float val);
                    Color bottom = Color.HSVToRGB(hue, sat, val * 0.78f);
                    bottom.a = top.a;
                    bg = new BackgroundSpec.Gradient(GradientTextureCache.Vertical(top, bottom));
                }
                else if (variant == ButtonVariant.Secondary && state.Hovered && !state.Pressed && !disabled) {
                    Color accent = theme.GetColor(ThemeSlot.SurfaceAccent);
                    Color top = new Color(accent.r, accent.g, accent.b, 0.42f);
                    Color bottom = new Color(accent.r * 0.62f, accent.g * 0.62f, accent.b * 0.62f, 0.28f);
                    bg = new BackgroundSpec.Gradient(GradientTextureCache.Vertical(top, bottom));
                }
                else {
                    bg = BackgroundSpec.Of(bgSlot.Value);
                }

                PaintBox.Draw(rect, bg, border, radius);
            }

            float overlay = ButtonVariants.OverlayAlpha(state);
            if (overlay > 0f) {
                Color overlayColor = InteractionFeedback.OverlayColor(theme, state, overlay);
                PaintBox.Draw(rect, BackgroundSpec.Of(overlayColor), null, radius);
            }

            bool rtl = dir == Direction.Rtl;
            Rect labelRect = new Rect(rect.x + padXPx, rect.y, rect.width - padXPx * 2f, rect.height);

            if (leading != null) {
                float leadingX = rtl
                    ? rect.xMax - padXPx - iconSize
                    : rect.x + padXPx;
                Rect leadingRect = new Rect(leadingX, rect.y + (rect.height - iconSize) / 2f, iconSize, iconSize);
                leading.MeasuredRect = leadingRect;
                if (rtl) {
                    labelRect = new Rect(
                        labelRect.x,
                        labelRect.y,
                        labelRect.width - (iconSize + iconGapPx),
                        labelRect.height
                    );
                }
                else {
                    labelRect = new Rect(
                        leadingX + iconSize + iconGapPx,
                        labelRect.y,
                        rect.xMax - padXPx - (leadingX + iconSize + iconGapPx),
                        labelRect.height
                    );
                }
            }

            if (trailing != null) {
                float trailingX = rtl
                    ? rect.x + padXPx
                    : rect.xMax - padXPx - iconSize;
                Rect trailingRect = new Rect(trailingX, rect.y + (rect.height - iconSize) / 2f, iconSize, iconSize);
                trailing.MeasuredRect = trailingRect;
                if (rtl) {
                    labelRect = new Rect(
                        trailingX + iconSize + iconGapPx,
                        labelRect.y,
                        labelRect.xMax - (trailingX + iconSize + iconGapPx),
                        labelRect.height
                    );
                }
                else {
                    labelRect = new Rect(labelRect.x, labelRect.y, trailingX - padXPx - labelRect.x, labelRect.height);
                }
            }

            if (body != null) {
                body.MeasuredRect = rect;
                LightweaveRoot.PaintSubtree(body, rect);
            }
            else {
                Color fg = s.TextColor switch {
                    ColorRef.Literal lit => lit.Value,
                    ColorRef.Token tok => theme.GetColor(tok.Slot),
                    _ => theme.GetColor(fgSlot),
                };

                Color savedColor = GUI.color;
                GUI.color = fg;
                GUI.Label(RectSnap.Snap(labelRect), label, gstyle);
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


    [DocVariant("CL_Playground_Label_Frosted")]
    public static DocSample DocsFrosted() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(() => Create("Frosted", () => { }, ButtonVariant.Frosted, disabled: forced));
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
