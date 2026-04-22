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

namespace Cosmere.Core.UI.Lightweave.Navigation;

public static class Menu
{
    private static readonly Rem RowHeight = new Rem(1.75f);
    private static readonly Rem DefaultWidth = new Rem(15f);
    private static readonly Rem RowPadding = new Rem(0.5f);
    private static readonly Rem IconSize = new Rem(1f);

    public static LightweaveNode Create(
        bool isOpen,
        Rect anchorRect,
        IReadOnlyList<MenuItem> items,
        Action onDismiss,
        object? instanceKey = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        string keySuffix = instanceKey == null ? string.Empty : "#" + instanceKey;
        string focusedKey = file + "#focused" + keySuffix;
        string submenuKey = file + "#submenu" + keySuffix;

        if (!isOpen)
        {
            LightweaveNode empty = NodeBuilder.New("Menu:closed", line, file);
            empty.Paint = (_, _) => { };
            return empty;
        }

        Hooks.Hooks.StateHandle<int> focusedIndex = Hooks.Hooks.UseState<int>(0, line, focusedKey);
        Hooks.Hooks.StateHandle<int> openSubmenuIndex = Hooks.Hooks.UseState<int>(-1, line, submenuKey);

        LightweaveNode content = NodeBuilder.New("Menu:content", line, file);
        content.Paint = (rect, _) =>
        {
            HandleKeyboard(items, focusedIndex, openSubmenuIndex, onDismiss);

            float rowH = RowHeight.ToPixels();
            int count = items.Count;
            for (int i = 0; i < count; i++)
            {
                Rect rowRect = new Rect(rect.x, rect.y + i * rowH, rect.width, rowH);
                PaintRow(rowRect, items[i], i, focusedIndex, openSubmenuIndex, onDismiss);
            }

            int submenuIdx = openSubmenuIndex.Value;
            if (submenuIdx >= 0 && submenuIdx < count)
            {
                MenuItem submenuOwner = items[submenuIdx];
                if (submenuOwner.Children != null && submenuOwner.Children.Count > 0)
                {
                    Rect submenuAnchor = new Rect(rect.x, rect.y + submenuIdx * rowH, rect.width, rowH);
                    LightweaveNode submenu = Create(
                        isOpen: true,
                        anchorRect: submenuAnchor,
                        items: submenuOwner.Children,
                        onDismiss: () => openSubmenuIndex.Set(-1),
                        instanceKey: instanceKey == null ? (object)submenuIdx : (instanceKey, submenuIdx));
                    submenu.MeasuredRect = submenuAnchor;
                    LightweaveRoot.PaintSubtree(submenu, submenuAnchor);
                }
            }
        };

        Vector2 size = new Vector2(DefaultWidth.ToPixels(), items.Count * RowHeight.ToPixels());
        return Popover.Create(
            isOpen: true,
            anchorRect: anchorRect,
            placement: PopoverPlacement.Bottom,
            content: content,
            onDismiss: onDismiss,
            preferredSize: size,
            caller: file,
            line: line);
    }

    private static void PaintRow(
        Rect rowRect,
        MenuItem item,
        int index,
        Hooks.Hooks.StateHandle<int> focusedIndex,
        Hooks.Hooks.StateHandle<int> openSubmenuIndex,
        Action onDismiss)
    {
        Theme.Theme theme = RenderContext.Current.Theme;
        Direction dir = RenderContext.Current.Direction;
        Event e = Event.current;
        bool hovering = !item.Disabled && rowRect.Contains(e.mousePosition);
        bool focused = focusedIndex.Value == index;
        bool hasChildren = item.Children != null && item.Children.Count > 0;

        if (hovering)
        {
            if (hasChildren)
            {
                if (openSubmenuIndex.Value != index)
                {
                    openSubmenuIndex.Set(index);
                }
            }
            else
            {
                if (openSubmenuIndex.Value != -1)
                {
                    openSubmenuIndex.Set(-1);
                }
            }
            if (focusedIndex.Value != index)
            {
                focusedIndex.Set(index);
            }
        }

        if (!item.Disabled && hovering)
        {
            BackgroundSpec hoverBg = new BackgroundSpec.Solid(ThemeSlot.SurfaceAccent);
            PaintBox.Draw(rowRect, hoverBg, null, null);
        }

        if (focused && !item.Disabled)
        {
            BorderSpec focusBorder = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderFocus);
            PaintBox.Draw(rowRect, null, focusBorder, null);
        }

        float padPx = RowPadding.ToPixels();
        float iconPx = IconSize.ToPixels();
        bool rtl = dir == Direction.Rtl;

        float labelStartX = rowRect.x + padPx;
        float labelEndX = rowRect.xMax - padPx;

        if (item.Icon != null)
        {
            float iconX = rtl
                ? rowRect.xMax - padPx - iconPx
                : rowRect.x + padPx;
            Rect iconRect = new Rect(iconX, rowRect.y + (rowRect.height - iconPx) / 2f, iconPx, iconPx);
            item.Icon.MeasuredRect = iconRect;
            LightweaveRoot.PaintSubtree(item.Icon, iconRect);
            if (rtl)
            {
                labelEndX = iconX - padPx;
            }
            else
            {
                labelStartX = iconX + iconPx + padPx;
            }
        }

        if (hasChildren)
        {
            float chevronPx = iconPx;
            float chevronX = rtl
                ? rowRect.x + padPx
                : rowRect.xMax - padPx - chevronPx;
            Rect chevronRect = new Rect(chevronX, rowRect.y, chevronPx, rowRect.height);
            Font chevronFont = theme.GetFont(FontRole.Body);
            int chevronSize = Mathf.RoundToInt(new Rem(1.25f).ToPixels());
            GUIStyle chevronStyle = GuiStyleCache.Get(chevronFont, chevronSize, FontStyle.Normal);
            chevronStyle.alignment = rtl ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            Color chevronSaved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            string chevronGlyph = rtl ? "‹" : "›";
            GUI.Label(RectSnap.Snap(chevronRect), chevronGlyph, chevronStyle);
            GUI.color = chevronSaved;
            if (rtl)
            {
                labelStartX = chevronX + chevronPx + padPx;
            }
            else
            {
                labelEndX = chevronX - padPx;
            }
        }

        Rect labelRect = new Rect(labelStartX, rowRect.y, labelEndX - labelStartX, rowRect.height);
        Font labelFont = theme.GetFont(FontRole.Body);
        int labelSize = Mathf.RoundToInt(new Rem(0.875f).ToPixels());
        GUIStyle labelStyle = GuiStyleCache.Get(labelFont, labelSize, FontStyle.Normal);
        labelStyle.alignment = Typography.Typography.ResolveAnchor(TextAlign.Start, dir);
        ThemeSlot labelSlot = item.Disabled ? ThemeSlot.TextMuted : ThemeSlot.TextPrimary;
        Color savedColor = GUI.color;
        GUI.color = theme.GetColor(labelSlot);
        GUI.Label(RectSnap.Snap(labelRect), item.Label, labelStyle);
        GUI.color = savedColor;

        if (!item.Disabled
            && e.type == EventType.MouseUp
            && e.button == 0
            && rowRect.Contains(e.mousePosition))
        {
            if (hasChildren)
            {
                openSubmenuIndex.Set(index);
            }
            else
            {
                item.OnInvoke?.Invoke();
                onDismiss?.Invoke();
            }
            e.Use();
        }
    }

    private static void HandleKeyboard(
        IReadOnlyList<MenuItem> items,
        Hooks.Hooks.StateHandle<int> focusedIndex,
        Hooks.Hooks.StateHandle<int> openSubmenuIndex,
        Action onDismiss)
    {
        Event e = Event.current;
        if (e.type != EventType.KeyDown)
        {
            return;
        }
        Direction dir = RenderContext.Current.Direction;
        bool rtl = dir == Direction.Rtl;
        int count = items.Count;
        int current = focusedIndex.Value;
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
                focusedIndex.Set(Math.Max(0, current - 1));
                e.Use();
                break;
            case KeyCode.DownArrow:
                focusedIndex.Set(Math.Min(count - 1, current + 1));
                e.Use();
                break;
            case KeyCode.Return:
            case KeyCode.KeypadEnter:
                if (current >= 0 && current < count && !items[current].Disabled)
                {
                    MenuItem chosen = items[current];
                    if (chosen.Children != null && chosen.Children.Count > 0)
                    {
                        openSubmenuIndex.Set(current);
                    }
                    else
                    {
                        chosen.OnInvoke?.Invoke();
                        onDismiss?.Invoke();
                    }
                }
                e.Use();
                break;
            case KeyCode.Escape:
                onDismiss?.Invoke();
                e.Use();
                break;
            case KeyCode.RightArrow:
                if (!rtl && current >= 0 && current < count
                    && items[current].Children != null
                    && items[current].Children!.Count > 0)
                {
                    openSubmenuIndex.Set(current);
                    e.Use();
                }
                break;
            case KeyCode.LeftArrow:
                if (rtl && current >= 0 && current < count
                    && items[current].Children != null
                    && items[current].Children!.Count > 0)
                {
                    openSubmenuIndex.Set(current);
                    e.Use();
                }
                break;
        }
    }
}
