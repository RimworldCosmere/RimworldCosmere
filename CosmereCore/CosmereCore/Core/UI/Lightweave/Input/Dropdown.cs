// NOTE: v1 Dropdown requires a valid initial value; there is no placeholder
// rendering when options are empty. Callers should pick a sensible default.
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Overlay;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Input;

public static class Dropdown
{
    private static readonly Rem RowHeight = new Rem(1.75f);
    private static readonly Rem RowPadding = new Rem(0.5f);
    private static readonly Rem ChevronWidth = new Rem(1.25f);
    private const int MaxVisibleRows = 10;
    private const float TypeAheadTimeoutSeconds = 1f;

    public static LightweaveNode Create<T>(
        T value,
        IReadOnlyList<T> options,
        Func<T, string> labelFn,
        Action<T> onChange,
        bool disabled = false,
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0)
    {
        LightweaveNode node = NodeBuilder.New("Dropdown", line, caller ?? string.Empty);

        node.Paint = (rect, paintChildren) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;

            Hooks.Hooks.StateHandle<bool> isOpen = Hooks.Hooks.UseState<bool>(false);
            Hooks.Hooks.StateHandle<int> highlightedIndex = Hooks.Hooks.UseState<int>(CurrentIndex(options, value));
            Hooks.Hooks.StateHandle<string> typeAheadBuffer = Hooks.Hooks.UseState<string>(string.Empty);
            Hooks.Hooks.RefHandle<float> typeAheadExpiry = Hooks.Hooks.UseRef<float>(0f);

            InteractionState state = InteractionState.Resolve(rect, focusName: null, disabled: disabled);
            InputSurface.Draw(rect, theme, state);

            float padPx = RowPadding.ToPixels();
            float chevronPx = ChevronWidth.ToPixels();
            bool rtl = dir == Direction.Rtl;

            float chevronX = rtl
                ? rect.x + padPx
                : rect.xMax - padPx - chevronPx;
            Rect chevronRect = new Rect(chevronX, rect.y, chevronPx, rect.height);

            float labelStartX = rtl ? chevronX + chevronPx + padPx : rect.x + padPx;
            float labelEndX = rtl ? rect.xMax - padPx : chevronX - padPx;
            Rect labelRect = new Rect(labelStartX, rect.y, labelEndX - labelStartX, rect.height);

            Font labelFont = theme.GetFont(FontRole.Body);
            int labelPixelSize = Mathf.RoundToInt(new Rem(0.875f).ToPixels());
            GUIStyle labelStyle = GuiStyleCache.Get(labelFont, labelPixelSize, FontStyle.Normal);
            labelStyle.alignment = Typography.Typography.ResolveAnchor(TextAlign.Start, dir);
            ThemeSlot labelSlot = disabled ? ThemeSlot.TextMuted : ThemeSlot.TextPrimary;
            string labelText = labelFn(value);

            Color savedColor = GUI.color;
            GUI.color = theme.GetColor(labelSlot);
            GUI.Label(RectSnap.Snap(labelRect), labelText, labelStyle);
            GUI.color = savedColor;

            Font chevronFont = theme.GetFont(FontRole.Body);
            int chevronPixelSize = Mathf.RoundToInt(new Rem(1.25f).ToPixels());
            GUIStyle chevronStyle = GuiStyleCache.Get(chevronFont, chevronPixelSize, FontStyle.Normal);
            chevronStyle.alignment = TextAnchor.MiddleCenter;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            GUI.Label(RectSnap.Snap(chevronRect), "▾", chevronStyle);
            GUI.color = savedColor;

            Event e = Event.current;

            if (!disabled
                && e.type == EventType.MouseUp
                && e.button == 0
                && rect.Contains(e.mousePosition))
            {
                isOpen.Set(!isOpen.Value);
                if (isOpen.Value)
                {
                    highlightedIndex.Set(CurrentIndex(options, value));
                }
                e.Use();
            }

            if (!disabled
                && !isOpen.Value
                && e.type == EventType.KeyDown
                && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                && rect.Contains(RenderContext.Current.PointerPos))
            {
                isOpen.Set(true);
                highlightedIndex.Set(CurrentIndex(options, value));
                e.Use();
            }

            if (isOpen.Value && options.Count > 0)
            {
                Rect anchor = rect;
                int capturedHighlight = highlightedIndex.Value;
                RenderContext.Current.PendingOverlays.Enqueue(() =>
                {
                    DrawPopover(
                        anchor,
                        options,
                        labelFn,
                        value,
                        capturedHighlight,
                        highlightedIndex,
                        typeAheadBuffer,
                        typeAheadExpiry,
                        onChange,
                        isOpen);
                });
            }

            paintChildren();
        };

        return node;
    }

    private static void DrawPopover<T>(
        Rect anchor,
        IReadOnlyList<T> options,
        Func<T, string> labelFn,
        T currentValue,
        int capturedHighlight,
        Hooks.Hooks.StateHandle<int> highlightedIndex,
        Hooks.Hooks.StateHandle<string> typeAheadBuffer,
        Hooks.Hooks.RefHandle<float> typeAheadExpiry,
        Action<T> onChange,
        Hooks.Hooks.StateHandle<bool> isOpen)
    {
        Theme.Theme theme = RenderContext.Current.Theme;
        Direction dir = RenderContext.Current.Direction;

        float rowH = RowHeight.ToPixels();
        int visibleCount = Mathf.Min(options.Count, MaxVisibleRows);
        float popoverHeight = visibleCount * rowH;

        Vector2 size = new Vector2(anchor.width, popoverHeight);
        Rect screen = new Rect(0f, 0f, Screen.width, Screen.height);
        Rect popoverRect = PopoverLayout.Resolve(anchor, PopoverPlacement.Bottom, dir, size, screen);

        BackgroundSpec bg = new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised);
        BorderSpec border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
        RadiusSpec radius = RadiusSpec.All(new Rem(0.5f));
        PaintBox.Draw(popoverRect, bg, border, radius);

        HandleKeyboard(
            options,
            labelFn,
            capturedHighlight,
            highlightedIndex,
            typeAheadBuffer,
            typeAheadExpiry,
            onChange,
            isOpen);

        int renderCount = Mathf.Min(options.Count, MaxVisibleRows);
        for (int i = 0; i < renderCount; i++)
        {
            Rect rowRect = new Rect(popoverRect.x, popoverRect.y + i * rowH, popoverRect.width, rowH);
            PaintRow(
                rowRect,
                options,
                labelFn,
                currentValue,
                i,
                highlightedIndex,
                onChange,
                isOpen);
        }

        Event e = Event.current;
        if (e.type == EventType.MouseDown
            && !popoverRect.Contains(e.mousePosition)
            && !anchor.Contains(e.mousePosition))
        {
            isOpen.Set(false);
            e.Use();
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
        Hooks.Hooks.StateHandle<bool> isOpen)
    {
        Theme.Theme theme = RenderContext.Current.Theme;
        Direction dir = RenderContext.Current.Direction;
        Event e = Event.current;

        T option = options[index];
        bool hovering = rowRect.Contains(e.mousePosition);
        bool highlighted = highlightedIndex.Value == index;
        bool selected = EqualityComparer<T>.Default.Equals(option, currentValue);

        if (hovering && highlightedIndex.Value != index)
        {
            highlightedIndex.Set(index);
        }

        if (hovering)
        {
            BackgroundSpec hoverBg = new BackgroundSpec.Solid(ThemeSlot.SurfaceAccent);
            PaintBox.Draw(rowRect, hoverBg, null, null);
        }

        if (highlighted)
        {
            BorderSpec focusBorder = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderFocus);
            PaintBox.Draw(rowRect, null, focusBorder, null);
        }

        float padPx = RowPadding.ToPixels();
        float checkPx = new Rem(1f).ToPixels();
        bool rtl = dir == Direction.Rtl;

        float labelStartX = rowRect.x + padPx;
        float labelEndX = rowRect.xMax - padPx;

        if (selected)
        {
            float checkX = rtl
                ? rowRect.x + padPx
                : rowRect.xMax - padPx - checkPx;
            Rect checkRect = new Rect(checkX, rowRect.y, checkPx, rowRect.height);
            Font checkFont = theme.GetFont(FontRole.Body);
            int checkPixelSize = Mathf.RoundToInt(new Rem(1f).ToPixels());
            GUIStyle checkStyle = GuiStyleCache.Get(checkFont, checkPixelSize, FontStyle.Normal);
            checkStyle.alignment = TextAnchor.MiddleCenter;
            Color savedCheck = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
            GUI.Label(RectSnap.Snap(checkRect), "✓", checkStyle);
            GUI.color = savedCheck;
            if (rtl)
            {
                labelStartX = checkX + checkPx + padPx;
            }
            else
            {
                labelEndX = checkX - padPx;
            }
        }

        Rect labelRect = new Rect(labelStartX, rowRect.y, labelEndX - labelStartX, rowRect.height);
        Font labelFont = theme.GetFont(FontRole.Body);
        int labelPixelSize = Mathf.RoundToInt(new Rem(0.875f).ToPixels());
        GUIStyle labelStyle = GuiStyleCache.Get(labelFont, labelPixelSize, FontStyle.Normal);
        labelStyle.alignment = Typography.Typography.ResolveAnchor(TextAlign.Start, dir);

        Color savedColor = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
        GUI.Label(RectSnap.Snap(labelRect), labelFn(option), labelStyle);
        GUI.color = savedColor;

        if (e.type == EventType.MouseUp
            && e.button == 0
            && rowRect.Contains(e.mousePosition))
        {
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
        Hooks.Hooks.StateHandle<bool> isOpen)
    {
        Event e = Event.current;
        if (e.type != EventType.KeyDown)
        {
            return;
        }

        int count = options.Count;
        if (count == 0)
        {
            return;
        }

        int current = capturedHighlight;
        if (current < 0)
        {
            current = 0;
        }
        if (current >= count)
        {
            current = count - 1;
        }

        switch (e.keyCode)
        {
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
        if (ch != '\0' && !char.IsControl(ch))
        {
            float now = Time.realtimeSinceStartup;
            string buffer = typeAheadBuffer.Value ?? string.Empty;
            if (now > typeAheadExpiry.Current)
            {
                buffer = string.Empty;
            }
            buffer += char.ToLowerInvariant(ch);
            typeAheadBuffer.Set(buffer);
            typeAheadExpiry.Current = now + TypeAheadTimeoutSeconds;

            int match = FindPrefixMatch(options, labelFn, buffer);
            if (match >= 0)
            {
                highlightedIndex.Set(match);
            }
            e.Use();
        }
    }

    private static int FindPrefixMatch<T>(IReadOnlyList<T> options, Func<T, string> labelFn, string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
        {
            return -1;
        }
        int count = options.Count;
        for (int i = 0; i < count; i++)
        {
            string label = labelFn(options[i]) ?? string.Empty;
            if (label.Length >= prefix.Length
                && string.Compare(label, 0, prefix, 0, prefix.Length, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return i;
            }
        }
        return -1;
    }

    private static int CurrentIndex<T>(IReadOnlyList<T> options, T value)
    {
        int count = options.Count;
        for (int i = 0; i < count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(options[i], value))
            {
                return i;
            }
        }
        return 0;
    }
}
