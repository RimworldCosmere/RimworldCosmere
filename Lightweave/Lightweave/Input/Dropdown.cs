// NOTE: v1 Dropdown requires a valid initial value; there is no placeholder
// rendering when options are empty. Callers should pick a sensible default.

using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Overlay;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Hooks.Hooks;

namespace Cosmere.Lightweave.Input;

[Doc(
    Id = "dropdown",
    Summary = "Single-select menu that opens a popover list of options.",
    WhenToUse = "Pick one value from a small-to-medium static set when an inline radio group would crowd layout.",
    SourcePath = "Lightweave/Lightweave/Input/Dropdown.cs"
)]
public static class Dropdown {
    private const int MaxVisibleRows = 10;
    private const float TypeAheadTimeoutSeconds = 1f;
    private static readonly Rem RowHeight = new Rem(1.75f);
    private static readonly Rem RowPadding = new Rem(0.5f);
    private static readonly Rem ChevronWidth = new Rem(1.25f);

    public static LightweaveNode Create<T>(
        [DocParam("The currently selected value.")]
        T value,
        [DocParam("All selectable values, in display order.")]
        IReadOnlyList<T> options,
        [DocParam("Maps an option to its display label.")]
        Func<T, string> labelFn,
        [DocParam("Invoked when the user picks a different option.")]
        Action<T> onChange,
        [DocParam("Trigger surface treatment: Input renders an input-style chrome, Inline renders compactly.")]
        DropdownVariant variant = DropdownVariant.Input,
        [DocParam("Button variant when variant=Button. Ignored for other variants.")]
        ButtonVariant buttonStyle = ButtonVariant.Secondary,
        [DocParam("Disable interaction and mute the trigger.")]
        bool disabled = false,
        [DocParam("Disambiguator when multiple dropdowns share the same caller line.")]
        object? instanceKey = null,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        string keySuffix = instanceKey == null ? string.Empty : "#" + instanceKey;
        string isOpenKey = file + "#dropdown_isOpen" + keySuffix;
        string highlightKey = file + "#dropdown_highlight" + keySuffix;
        string typeAheadKey = file + "#dropdown_typeAhead" + keySuffix;
        string typeAheadExpiryKey = file + "#dropdown_typeAheadExpiry" + keySuffix;

        LightweaveNode node = NodeBuilder.New("Dropdown", line, file);
        node.ApplyStyling("dropdown", style, classes, id);
        node.PreferredHeight = RowHeight.ToPixels();

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;

            Hooks.Hooks.StateHandle<bool> isOpen = Hooks.Hooks.UseState(false, line, isOpenKey);
            Hooks.Hooks.StateHandle<int> highlightedIndex = Hooks.Hooks.UseState(
                CurrentIndex(options, value),
                line,
                highlightKey
            );
            Hooks.Hooks.StateHandle<string> typeAheadBuffer = Hooks.Hooks.UseState(
                string.Empty,
                line,
                typeAheadKey
            );
            Hooks.Hooks.RefHandle<float> typeAheadExpiry = Hooks.Hooks.UseRef(0f, line, typeAheadExpiryKey);

            if (isOpen.Value && SingletonOverlayRegistry.ShouldClose(isOpenKey)) {
                isOpen.Set(false);
            }

            InteractionState state = InteractionState.Resolve(rect, null, disabled);
            TriggerStyle style = PaintTriggerSurface(rect, variant, buttonStyle, state, disabled);
            (Rect labelRect, Rect chevronRect) = ComputeTriggerLayout(rect, dir);
            DrawTriggerContent(labelRect, chevronRect, labelFn(value), variant, style, theme, dir);

            HandleTriggerInteraction(rect, disabled, options, value, isOpenKey, isOpen, highlightedIndex);

            if (isOpen.Value && options.Count > 0) {
                EnqueueOverlay(
                    rect,
                    options,
                    labelFn,
                    value,
                    isOpenKey,
                    isOpen,
                    highlightedIndex,
                    typeAheadBuffer,
                    typeAheadExpiry,
                    onChange
                );
            }

            paintChildren();
        };

        return node;
    }

    private struct TriggerStyle {
        public ThemeSlot LabelSlot;
        public ThemeSlot ChevronSlot;
        public FontRole LabelFontRole;
        public FontStyle LabelFontStyle;
    }

    private static TriggerStyle PaintTriggerSurface(
        Rect rect,
        DropdownVariant variant,
        ButtonVariant buttonStyle,
        InteractionState state,
        bool disabled
    ) {
        if (variant == DropdownVariant.Button) {
            ThemeSlot fgSlot = ButtonVariants.Foreground(buttonStyle, state);
            ThemeSlot? borderSlot = ButtonVariants.Border(buttonStyle, state);
            BorderSpec? borderSpec = borderSlot.HasValue
                ? BorderSpec.All(new Rem(1f / 16f), borderSlot.Value)
                : null;
            RadiusSpec radiusSpec = RadiusSpec.All(RadiusScale.Sm);

            if (buttonStyle == ButtonVariant.Frosted) {
                bool active = state.Hovered || state.Pressed;
                BackdropBlur.Draw(rect, active ? 8f : 6f);
                Color translucent = new Color(20f / 255f, 16f / 255f, 11f / 255f, active ? 0.88f : 0.78f);
                PaintBox.Draw(rect, BackgroundSpec.Of(translucent), borderSpec, radiusSpec);
            }
            else {
                ThemeSlot? bgSlot = ButtonVariants.Background(buttonStyle, state);
                BackgroundSpec? bgSpec = bgSlot.HasValue ? BackgroundSpec.Of(bgSlot.Value) : null;
                PaintBox.Draw(rect, bgSpec, borderSpec, radiusSpec);
            }

            float overlay = ButtonVariants.OverlayAlpha(state);
            if (overlay > 0f) {
                Color overlayColor = InteractionFeedback.OverlayColor(RenderContext.Current.Theme, state, overlay);
                PaintBox.Draw(rect, BackgroundSpec.Of(overlayColor), null, radiusSpec);
            }

            return new TriggerStyle {
                LabelSlot = fgSlot,
                ChevronSlot = fgSlot,
                LabelFontRole = FontRole.BodyBold,
                LabelFontStyle = FontStyle.Normal
            };
        }

        InputSurface.Draw(rect, state);
        return new TriggerStyle {
            LabelSlot = disabled ? ThemeSlot.TextMuted : ThemeSlot.TextPrimary,
            ChevronSlot = ThemeSlot.TextMuted,
            LabelFontRole = FontRole.Body,
            LabelFontStyle = FontStyle.Normal
        };
    }

    private static (Rect labelRect, Rect chevronRect) ComputeTriggerLayout(Rect rect, Direction dir) {
        float padPx = RowPadding.ToPixels();
        float chevronPx = ChevronWidth.ToPixels();
        bool rtl = dir == Direction.Rtl;

        float chevronX = rtl ? rect.x + padPx : rect.xMax - padPx - chevronPx;
        Rect chevronRect = new Rect(chevronX, rect.y, chevronPx, rect.height);

        float labelStartX = rtl ? chevronX + chevronPx + padPx : rect.x + padPx;
        float labelEndX = rtl ? rect.xMax - padPx : chevronX - padPx;
        Rect labelRect = new Rect(labelStartX, rect.y, labelEndX - labelStartX, rect.height);

        return (labelRect, chevronRect);
    }

    private static void DrawTriggerContent(
        Rect labelRect,
        Rect chevronRect,
        string labelText,
        DropdownVariant variant,
        TriggerStyle style,
        Theme.Theme theme,
        Direction dir
    ) {
        Font labelFont = theme.GetFont(style.LabelFontRole);
        int labelPixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
        GUIStyle labelStyle = GuiStyleCache.GetOrCreate(labelFont, labelPixelSize, style.LabelFontStyle);
        labelStyle.alignment = variant == DropdownVariant.Button
            ? TextAnchor.MiddleCenter
            : Typography.Typography.ResolveAnchor(TextAlign.Start, dir);

        Color savedColor = GUI.color;
        GUI.color = theme.GetColor(style.LabelSlot);
        GUI.Label(RectSnap.Snap(labelRect), labelText, labelStyle);
        GUI.color = savedColor;

        Font chevronFont = theme.GetFont(FontRole.Body);
        int chevronPixelSize = Mathf.RoundToInt(new Rem(1.25f).ToFontPx());
        GUIStyle chevronStyle = GuiStyleCache.GetOrCreate(chevronFont, chevronPixelSize);
        chevronStyle.alignment = TextAnchor.MiddleCenter;
        GUI.color = theme.GetColor(style.ChevronSlot);
        GUI.Label(RectSnap.Snap(chevronRect), "▾", chevronStyle);
        GUI.color = savedColor;
    }

    private static void HandleTriggerInteraction<T>(
        Rect rect,
        bool disabled,
        IReadOnlyList<T> options,
        T value,
        string isOpenKey,
        Hooks.Hooks.StateHandle<bool> isOpen,
        Hooks.Hooks.StateHandle<int> highlightedIndex
    ) {
        if (disabled) {
            return;
        }

        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition)) {
            e.Use();
            return;
        }

        if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
            bool willOpen = !isOpen.Value;
            isOpen.Set(willOpen);
            if (willOpen) {
                SingletonOverlayRegistry.Open(isOpenKey);
                highlightedIndex.Set(CurrentIndex(options, value));
            }
            else {
                SingletonOverlayRegistry.Close(isOpenKey);
            }

            e.Use();
            return;
        }

        if (!isOpen.Value &&
            e.type == EventType.KeyDown &&
            (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) &&
            rect.Contains(RenderContext.Current.PointerPos)) {
            isOpen.Set(true);
            SingletonOverlayRegistry.Open(isOpenKey);
            highlightedIndex.Set(CurrentIndex(options, value));
            e.Use();
        }
    }

    private static void EnqueueOverlay<T>(
        Rect rect,
        IReadOnlyList<T> options,
        Func<T, string> labelFn,
        T value,
        string isOpenKey,
        Hooks.Hooks.StateHandle<bool> isOpen,
        Hooks.Hooks.StateHandle<int> highlightedIndex,
        Hooks.Hooks.StateHandle<string> typeAheadBuffer,
        Hooks.Hooks.RefHandle<float> typeAheadExpiry,
        Action<T> onChange
    ) {
        Rect anchorAbsolute = OverlayAnchor.CaptureAbsolute(rect);
        int capturedHighlight = highlightedIndex.Value;
        string capturedKey = isOpenKey;
        RenderContext.Current.PendingOverlays.Enqueue(() => {
            Rect anchorHere = OverlayAnchor.ResolveLocal(anchorAbsolute);
            RenderOpenDropdown(
                anchorHere,
                options,
                labelFn,
                value,
                capturedHighlight,
                highlightedIndex,
                typeAheadBuffer,
                typeAheadExpiry,
                onChange,
                isOpen,
                capturedKey
            );
        }
        );
    }

    private static void RenderOpenDropdown<T>(
        Rect anchor,
        IReadOnlyList<T> options,
        Func<T, string> labelFn,
        T currentValue,
        int capturedHighlight,
        Hooks.Hooks.StateHandle<int> highlightedIndex,
        Hooks.Hooks.StateHandle<string> typeAheadBuffer,
        Hooks.Hooks.RefHandle<float> typeAheadExpiry,
        Action<T> onChange,
        Hooks.Hooks.StateHandle<bool> isOpen,
        string dropdownKey
    ) {
        Theme.Theme theme = RenderContext.Current.Theme;
        Direction dir = RenderContext.Current.Direction;

        float rowH = RowHeight.ToPixels();
        int visibleCount = Mathf.Min(options.Count, MaxVisibleRows);
        float popoverHeight = visibleCount * rowH;

        Vector2 size = new Vector2(anchor.width, popoverHeight);
        Rect screen = new Rect(0f, 0f, Screen.width, Screen.height);
        Rect popoverRect = PopoverLayout.Resolve(anchor, PopoverPlacement.Bottom, dir, size, screen);

        Color savedOverlayColor = GUI.color;
        GUI.color = Color.white;

        Rect shadowRect = new Rect(popoverRect.x + 2f, popoverRect.y + 3f, popoverRect.width, popoverRect.height);
        BackgroundSpec shadowBg = BackgroundSpec.Of(ThemeSlot.SurfaceShadow);
        PaintBox.Draw(shadowRect, shadowBg, null, RadiusSpec.All(RadiusScale.Lg));

        BackgroundSpec bg = BackgroundSpec.Of(ThemeSlot.SurfaceRaised);
        BorderSpec border = BorderSpec.All(new Rem(2f / 16f), ThemeSlot.BorderDefault);
        RadiusSpec radius = RadiusSpec.All(RadiusScale.Lg);
        PaintBox.Draw(popoverRect, bg, border, radius);

        GUI.color = savedOverlayColor;

        HandleKeyboard(
            options,
            labelFn,
            capturedHighlight,
            highlightedIndex,
            typeAheadBuffer,
            typeAheadExpiry,
            onChange,
            isOpen
        );

        for (int i = 0; i < visibleCount; i++) {
            Rect rowRect = new Rect(popoverRect.x, popoverRect.y + i * rowH, popoverRect.width, rowH);
            PaintRow(
                rowRect,
                options,
                labelFn,
                currentValue,
                i,
                highlightedIndex,
                onChange,
                isOpen
            );
        }

        Event e = Event.current;
        if (e.rawType == EventType.MouseDown &&
            !popoverRect.Contains(e.mousePosition) &&
            !anchor.Contains(e.mousePosition)) {
            isOpen.Set(false);
            SingletonOverlayRegistry.Close(dropdownKey);

            if (e.type == EventType.MouseDown) {
                e.Use();
            }
        }
    }

    private static void PaintRow<T>(
        Rect rowRect,
        IReadOnlyList<T> options,
        Func<T, string> labelFn,
        T currentValue,
        int index,
        Hooks.Hooks.StateHandle<int> highlightedIndex,
        Action<T> onChange,
        Hooks.Hooks.StateHandle<bool> isOpen
    ) {
        Theme.Theme theme = RenderContext.Current.Theme;
        Direction dir = RenderContext.Current.Direction;
        Event e = Event.current;

        T option = options[index];
        bool hovering = rowRect.Contains(e.mousePosition);
        bool highlighted = highlightedIndex.Value == index;
        bool selected = EqualityComparer<T>.Default.Equals(option, currentValue);

        if (hovering && highlightedIndex.Value != index) {
            highlightedIndex.Set(index);
        }

        float highlightInset = new Rem(0.125f).ToPixels();
        Rect highlightRect = new Rect(
            rowRect.x + highlightInset,
            rowRect.y + highlightInset,
            Mathf.Max(0f, rowRect.width - highlightInset * 2f),
            Mathf.Max(0f, rowRect.height - highlightInset * 2f)
        );
        RadiusSpec highlightRadius = RadiusSpec.All(RadiusScale.Lg);

        if (hovering) {
            BackgroundSpec hoverBg = BackgroundSpec.Of(ThemeSlot.SurfaceAccent);
            PaintBox.Draw(highlightRect, hoverBg, null, highlightRadius);
        }

        if (highlighted && !hovering) {
            BackgroundSpec focusFill = BackgroundSpec.Of(ThemeSlot.SurfaceRaised);
            BorderSpec focusBorder = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderFocus);
            PaintBox.Draw(highlightRect, focusFill, focusBorder, highlightRadius);
        }

        float padPx = RowPadding.ToPixels();
        float checkPx = new Rem(1f).ToPixels();
        bool rtl = dir == Direction.Rtl;

        float labelStartX = rowRect.x + padPx;
        float labelEndX = rowRect.xMax - padPx;

        if (selected) {
            float checkX = rtl
                ? rowRect.x + padPx
                : rowRect.xMax - padPx - checkPx;
            Rect checkRect = new Rect(checkX, rowRect.y, checkPx, rowRect.height);
            Font checkFont = theme.GetFont(FontRole.Body);
            int checkPixelSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
            GUIStyle checkStyle = GuiStyleCache.GetOrCreate(checkFont, checkPixelSize);
            checkStyle.alignment = TextAnchor.MiddleCenter;
            Color savedCheck = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
            GUI.Label(RectSnap.Snap(checkRect), "✓", checkStyle);
            GUI.color = savedCheck;
            if (rtl) {
                labelStartX = checkX + checkPx + padPx;
            }
            else {
                labelEndX = checkX - padPx;
            }
        }

        Rect labelRect = new Rect(labelStartX, rowRect.y, labelEndX - labelStartX, rowRect.height);
        Font labelFont = theme.GetFont(FontRole.Body);
        int labelPixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
        GUIStyle labelStyle = GuiStyleCache.GetOrCreate(labelFont, labelPixelSize);
        labelStyle.alignment = Typography.Typography.ResolveAnchor(TextAlign.Start, dir);

        Color savedColor = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
        GUI.Label(RectSnap.Snap(labelRect), labelFn(option), labelStyle);
        GUI.color = savedColor;

        if (e.type == EventType.MouseDown && e.button == 0 && rowRect.Contains(e.mousePosition)) {
            e.Use();
        }

        if (e.type == EventType.MouseUp && e.button == 0 && rowRect.Contains(e.mousePosition)) {
            onChange?.Invoke(option);
            isOpen.Set(false);
            e.Use();
        }
    }

    private static void HandleKeyboard<T>(
        IReadOnlyList<T> options,
        Func<T, string> labelFn,
        int capturedHighlight,
        Hooks.Hooks.StateHandle<int> highlightedIndex,
        Hooks.Hooks.StateHandle<string> typeAheadBuffer,
        Hooks.Hooks.RefHandle<float> typeAheadExpiry,
        Action<T> onChange,
        Hooks.Hooks.StateHandle<bool> isOpen
    ) {
        Event e = Event.current;
        if (e.type != EventType.KeyDown) {
            return;
        }

        int count = options.Count;
        if (count == 0) {
            return;
        }

        int current = capturedHighlight;
        if (current < 0) {
            current = 0;
        }

        if (current >= count) {
            current = count - 1;
        }

        switch (e.keyCode) {
            case KeyCode.UpArrow:
                highlightedIndex.Set(Math.Max(0, current - 1));
                e.Use();
                return;
            case KeyCode.DownArrow:
                highlightedIndex.Set(Math.Min(count - 1, current + 1));
                e.Use();
                return;
            case KeyCode.Home:
                highlightedIndex.Set(0);
                e.Use();
                return;
            case KeyCode.End:
                highlightedIndex.Set(count - 1);
                e.Use();
                return;
            case KeyCode.Return:
            case KeyCode.KeypadEnter:
                onChange?.Invoke(options[current]);
                isOpen.Set(false);
                e.Use();
                return;
            case KeyCode.Escape:
                isOpen.Set(false);
                e.Use();
                return;
        }

        char ch = e.character;
        if (ch != '\0' && !char.IsControl(ch)) {
            float now = Time.realtimeSinceStartup;
            string buffer = typeAheadBuffer.Value ?? string.Empty;
            if (now > typeAheadExpiry.Current) {
                buffer = string.Empty;
            }

            buffer += char.ToLowerInvariant(ch);
            typeAheadBuffer.Set(buffer);
            typeAheadExpiry.Current = now + TypeAheadTimeoutSeconds;

            int match = FindPrefixMatch(options, labelFn, buffer);
            if (match >= 0) {
                highlightedIndex.Set(match);
            }

            e.Use();
        }
    }

    private static int FindPrefixMatch<T>(IReadOnlyList<T> options, Func<T, string> labelFn, string prefix) {
        if (string.IsNullOrEmpty(prefix)) {
            return -1;
        }

        int count = options.Count;
        for (int i = 0; i < count; i++) {
            string label = labelFn(options[i]);
            if (label.Length >= prefix.Length &&
                string.Compare(label, 0, prefix, 0, prefix.Length, StringComparison.OrdinalIgnoreCase) == 0) {
                return i;
            }
        }

        return -1;
    }

    private static int CurrentIndex<T>(IReadOnlyList<T> options, T value) {
        int count = options.Count;
        for (int i = 0; i < count; i++) {
            if (EqualityComparer<T>.Default.Equals(options[i], value)) {
                return i;
            }
        }

        return 0;
    }

    private static readonly string[] DocOptions = { "Roshar", "Scadrial", "Nalthis", "Taldain", "Ashyn" };

    [DocVariant("CL_Playground_Label_Input")]
    public static DocSample DocsInput() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<string> s = UseState("Scadrial");
        return new DocSample(() => Create<string>(
            s.Value,
            DocOptions,
            v => v,
            v => s.Set(v),
            disabled: forced,
            instanceKey: "doc-input"
        ));
    }

    [DocVariant("CL_Playground_Label_Button")]
    public static DocSample DocsButton() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<string> s = UseState("Scadrial");
        return new DocSample(() => Create<string>(
            s.Value,
            DocOptions,
            v => v,
            v => s.Set(v),
            DropdownVariant.Button,
            ButtonVariant.Secondary,
            forced,
            "doc-btn-secondary"
        ));
    }

    [DocVariant("CL_Playground_Label_Primary")]
    public static DocSample DocsPrimary() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<string> s = UseState("Scadrial");
        return new DocSample(() => Create<string>(
            s.Value,
            DocOptions,
            v => v,
            v => s.Set(v),
            DropdownVariant.Button,
            ButtonVariant.Primary,
            forced,
            "doc-btn-primary"
        ));
    }


    [DocVariant("CL_Playground_Label_Frosted")]
    public static DocSample DocsFrosted() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<string> s = UseState("Scadrial");
        return new DocSample(() => Create<string>(
            s.Value,
            DocOptions,
            v => v,
            v => s.Set(v),
            DropdownVariant.Button,
            ButtonVariant.Frosted,
            forced,
            "doc-btn-frosted"
        ));
    }

    [DocState("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<string> s = UseState("Scadrial");
        return new DocSample(() => Create<string>(
            s.Value,
            DocOptions,
            v => v,
            v => s.Set(v),
            disabled: forced,
            instanceKey: "doc-st-default"
        ));
    }

    [DocState("CL_Playground_Label_Hover")]
    public static DocSample DocsHover() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<string> s = UseState("Roshar");
        return new DocSample(() => Create<string>(
            s.Value,
            DocOptions,
            v => v,
            v => s.Set(v),
            disabled: forced,
            instanceKey: "doc-st-hover"
        ));
    }

    [DocState("CL_Playground_Label_Disabled")]
    public static DocSample DocsDisabled() {
        StateHandle<string> s = UseState("Nalthis");
        return new DocSample(() => Create<string>(
            s.Value,
            DocOptions,
            v => v,
            v => s.Set(v),
            disabled: true,
            instanceKey: "doc-st-disabled"
        ));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        StateHandle<string> s = UseState("Scadrial");
        return new DocSample(() => Create<string>(
            s.Value,
            DocOptions,
            v => v,
            v => s.Set(v)
        ));
    }
}